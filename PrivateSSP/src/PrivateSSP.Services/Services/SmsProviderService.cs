using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PrivateSSP.Core.Interfaces;
using PrivateSSP.Core.Models;
using System.Text.RegularExpressions;

namespace PrivateSSP.Services.Services;

/// <summary>
/// Service for sending SMS messages through various providers
/// </summary>
public class SmsProviderService : ISmsProviderService
{
    private readonly ILogger<SmsProviderService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public SmsProviderService(ILogger<SmsProviderService> logger, IConfiguration configuration, HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<SmsSendResult> SendSmsAsync(SmsMessage message)
    {
        try
        {
            _logger.LogInformation("Sending SMS to {PhoneNumber} with UUID {Uuid}", message.PhoneNumber, message.Uuid);

            // Validate phone number
            if (!await ValidatePhoneNumberAsync(message.PhoneNumber))
            {
                return new SmsSendResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid phone number format",
                    Status = SmsDeliveryStatus.Failed
                };
            }

            // Get provider configuration
            var provider = _configuration["SmsProvider:Name"] ?? "default";
            var apiKey = _configuration["SmsProvider:ApiKey"];
            var apiUrl = _configuration["SmsProvider:ApiUrl"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogError("SMS provider configuration is missing");
                return new SmsSendResult
                {
                    IsSuccess = false,
                    ErrorMessage = "SMS provider not configured",
                    Status = SmsDeliveryStatus.Failed
                };
            }

            // Prepare request payload
            var payload = new
            {
                to = message.PhoneNumber,
                message = message.Message,
                sender_id = message.SenderId,
                campaign_id = message.CampaignId,
                user_id = message.UserId
            };

            // Send SMS through provider
            var response = await SendSmsToProviderAsync(apiUrl, apiKey, payload);

            if (response.IsSuccess)
            {
                _logger.LogInformation("SMS sent successfully to {PhoneNumber}", message.PhoneNumber);
                return new SmsSendResult
                {
                    IsSuccess = true,
                    MessageId = response.MessageId,
                    Status = SmsDeliveryStatus.Sent
                };
            }

            return new SmsSendResult
            {
                IsSuccess = false,
                ErrorMessage = response.ErrorMessage,
                Status = SmsDeliveryStatus.Failed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", message.PhoneNumber);
            return new SmsSendResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message,
                Status = SmsDeliveryStatus.Failed
            };
        }
    }

    public async Task<SmsDeliveryStatus> GetDeliveryStatusAsync(string messageId)
    {
        try
        {
            var apiUrl = _configuration["SmsProvider:StatusUrl"];
            var apiKey = _configuration["SmsProvider:ApiKey"];

            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Status check URL not configured");
                return SmsDeliveryStatus.Pending;
            }

            // Implementation would depend on the specific SMS provider
            // This is a placeholder implementation
            _logger.LogInformation("Checking delivery status for message {MessageId}", messageId);
            
            return SmsDeliveryStatus.Delivered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get delivery status for message {MessageId}", messageId);
            return SmsDeliveryStatus.Failed;
        }
    }

    public async Task<decimal> GetBalanceAsync()
    {
        try
        {
            var apiUrl = _configuration["SmsProvider:BalanceUrl"];
            var apiKey = _configuration["SmsProvider:ApiKey"];

            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Balance check URL not configured");
                return 0;
            }

            // Implementation would depend on the specific SMS provider
            // This is a placeholder implementation
            _logger.LogInformation("Checking SMS provider balance");
            
            return 100.00m; // Placeholder balance
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get SMS provider balance");
            return 0;
        }
    }

    public async Task<bool> ValidatePhoneNumberAsync(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Basic phone number validation
        var phoneRegex = new Regex(@"^\+?[1-9]\d{1,14}$");
        return phoneRegex.IsMatch(phoneNumber);
    }

    private async Task<SmsSendResult> SendSmsToProviderAsync(string apiUrl, string apiKey, object payload)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Parse response to get message ID
                var responseData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                var messageId = responseData?.GetValueOrDefault("message_id")?.ToString();

                return new SmsSendResult
                {
                    IsSuccess = true,
                    MessageId = messageId,
                    Status = SmsDeliveryStatus.Sent
                };
            }

            return new SmsSendResult
            {
                IsSuccess = false,
                ErrorMessage = $"Provider returned {response.StatusCode}: {responseContent}",
                Status = SmsDeliveryStatus.Failed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to provider");
            return new SmsSendResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message,
                Status = SmsDeliveryStatus.Failed
            };
        }
    }
}
