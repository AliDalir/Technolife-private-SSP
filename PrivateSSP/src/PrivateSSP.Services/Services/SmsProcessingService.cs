using Microsoft.Extensions.Logging;
using PrivateSSP.Core.Interfaces;
using PrivateSSP.Core.Models;

namespace PrivateSSP.Services.Services;

/// <summary>
/// Main service for processing SMS messages through the entire workflow
/// </summary>
public class SmsProcessingService
{
    private readonly ILogger<SmsProcessingService> _logger;
    private readonly IDecryptionService _decryptionService;
    private readonly ISmsQueueService _queueService;
    private readonly ISmsProviderService _providerService;
    private readonly IWebhookService _webhookService;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _processingTask;

    public SmsProcessingService(
        ILogger<SmsProcessingService> logger,
        IDecryptionService decryptionService,
        ISmsQueueService queueService,
        ISmsProviderService providerService,
        IWebhookService webhookService)
    {
        _logger = logger;
        _decryptionService = decryptionService;
        _queueService = queueService;
        _providerService = providerService;
        _webhookService = webhookService;
        _cancellationTokenSource = new CancellationTokenSource();

        // Start the background processing task
        _processingTask = Task.Run(ProcessQueueAsync);
    }

    /// <summary>
    /// Processes an incoming SMS request from WebEngage
    /// </summary>
    public async Task<SmsResponse> ProcessSmsRequestAsync(SmsRequest request)
    {
        try
        {
            _logger.LogInformation("Processing SMS request {Uuid} for hashed phone {HashPrefix}...", 
                request.Uuid, request.HashedPhone[..8]);

            // Validate the request
            if (!await ValidateRequestAsync(request))
            {
                var response = new SmsResponse
                {
                    Status = SmsResponseStatus.Rejected,
                    StatusCode = 2001,
                    Message = "Invalid request data",
                    Uuid = request.Uuid
                };

                await SendWebhookNotificationAsync(request.Uuid, response.Status, response.StatusCode, response.Message);
                return response;
            }

            // Decrypt the phone number
            string decryptedPhone;
            try
            {
                decryptedPhone = await _decryptionService.DecryptPhoneNumberAsync(request.HashedPhone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt phone number for request {Uuid}", request.Uuid);
                
                var response = new SmsResponse
                {
                    Status = SmsResponseStatus.Rejected,
                    StatusCode = 2011,
                    Message = "Authentication failure - unable to decrypt phone number",
                    Uuid = request.Uuid
                };

                await SendWebhookNotificationAsync(request.Uuid, response.Status, response.StatusCode, response.Message);
                return response;
            }

            // Create SMS message
            var smsMessage = new SmsMessage
            {
                Uuid = request.Uuid,
                PhoneNumber = decryptedPhone,
                Message = request.Message,
                SenderId = request.SenderId,
                CampaignId = request.CampaignId,
                UserId = request.UserId,
                CustomData = request.CustomData,
                Priority = DeterminePriority(request),
                Status = SmsMessageStatus.Pending
            };

            // Add to queue
            var queued = await _queueService.EnqueueMessageAsync(smsMessage);
            if (!queued)
            {
                var response = new SmsResponse
                {
                    Status = SmsResponseStatus.Rejected,
                    StatusCode = 2015,
                    Message = "Throttling error - queue is full",
                    Uuid = request.Uuid
                };

                await SendWebhookNotificationAsync(request.Uuid, response.Status, response.StatusCode, response.Message);
                return response;
            }

            // Return success response
            var successResponse = new SmsResponse
            {
                Status = SmsResponseStatus.Success,
                StatusCode = 1000,
                Message = "SMS accepted and queued for delivery",
                Uuid = request.Uuid
            };

            _logger.LogInformation("SMS request {Uuid} processed successfully and queued", request.Uuid);
            return successResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process SMS request {Uuid}", request.Uuid);
            
            var response = new SmsResponse
            {
                Status = SmsResponseStatus.Rejected,
                StatusCode = 9988,
                Message = "Unknown reason - internal server error",
                Uuid = request.Uuid
            };

            await SendWebhookNotificationAsync(request.Uuid, response.Status, response.StatusCode, response.Message);
            return response;
        }
    }

    /// <summary>
    /// Background task for processing queued messages
    /// </summary>
    private async Task ProcessQueueAsync()
    {
        _logger.LogInformation("SMS processing service started");

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                // Get next message from queue
                var message = await _queueService.DequeueMessageAsync();
                if (message == null)
                {
                    await Task.Delay(1000, _cancellationTokenSource.Token); // Wait 1 second
                    continue;
                }

                _logger.LogInformation("Processing queued message {Uuid} for {PhoneNumber}", 
                    message.Uuid, message.PhoneNumber);

                // Update status to sending
                message.Status = SmsMessageStatus.Sending;

                // Send SMS through provider
                var sendResult = await _providerService.SendSmsAsync(message);

                if (sendResult.IsSuccess)
                {
                    message.Status = SmsMessageStatus.Sent;
                    _logger.LogInformation("SMS {Uuid} sent successfully", message.Uuid);

                    // Send webhook notification
                    await SendWebhookNotificationAsync(message.Uuid, "sms_delivered", 1001, "Message delivered successfully");
                }
                else
                {
                    message.Status = SmsMessageStatus.Failed;
                    _logger.LogWarning("SMS {Uuid} failed to send: {Error}", message.Uuid, sendResult.ErrorMessage);

                    // Check if we should retry
                    if (message.RetryCount < message.MaxRetries)
                    {
                        message.RetryCount++;
                        message.Status = SmsMessageStatus.Pending;
                        await _queueService.EnqueueMessageAsync(message);
                        _logger.LogInformation("SMS {Uuid} requeued for retry {RetryCount}", message.Uuid, message.RetryCount);
                    }
                    else
                    {
                        // Send webhook notification for final failure
                        await SendWebhookNotificationAsync(message.Uuid, "sms_failed", 2009, 
                            $"Message failed after {message.MaxRetries} retries: {sendResult.ErrorMessage}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SMS processing loop");
                await Task.Delay(5000, _cancellationTokenSource.Token); // Wait 5 seconds on error
            }
        }

        _logger.LogInformation("SMS processing service stopped");
    }

    private async Task<bool> ValidateRequestAsync(SmsRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Uuid) || 
            string.IsNullOrWhiteSpace(request.HashedPhone) || string.IsNullOrWhiteSpace(request.Message))
        {
            return false;
        }

        if (request.Message.Length > 160) // Basic SMS length validation
        {
            return false;
        }

        return await _decryptionService.ValidateHashedValueAsync(request.HashedPhone);
    }

    private int DeterminePriority(SmsRequest request)
    {
        // Simple priority logic - can be enhanced based on business rules
        if (request.CustomData?.ContainsKey("priority") == true)
        {
            if (int.TryParse(request.CustomData["priority"].ToString(), out var priority))
            {
                return Math.Max(1, Math.Min(5, priority));
            }
        }

        return 3; // Default priority
    }

    private async Task SendWebhookNotificationAsync(string uuid, string status, int statusCode, string message)
    {
        try
        {
            await _webhookService.SendDeliveryStatusNotificationAsync(uuid, status, statusCode, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook notification for {Uuid}", uuid);
        }
    }

    /// <summary>
    /// Stops the SMS processing service
    /// </summary>
    public async Task StopAsync()
    {
        _logger.LogInformation("Stopping SMS processing service");
        _cancellationTokenSource.Cancel();
        
        try
        {
            await _processingTask;
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }
    }
}
