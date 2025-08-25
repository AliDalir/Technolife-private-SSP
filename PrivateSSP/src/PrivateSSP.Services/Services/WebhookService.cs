using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PrivateSSP.Core.Interfaces;
using PrivateSSP.Core.Models;

namespace PrivateSSP.Services.Services;

/// <summary>
/// Service for sending webhook notifications to WebEngage
/// </summary>
public class WebhookService : IWebhookService
{
    private readonly ILogger<WebhookService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public WebhookService(ILogger<WebhookService> logger, IConfiguration configuration, HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<bool> SendDeliveryStatusNotificationAsync(string uuid, string status, int statusCode, string message)
    {
        try
        {
            var webhookUrl = GetWebhookUrl();
            if (string.IsNullOrEmpty(webhookUrl))
            {
                _logger.LogError("Webhook URL not configured");
                return false;
            }

            var notification = new DeliveryStatusNotification
            {
                Uuid = uuid,
                Status = status,
                StatusCode = statusCode,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            var json = System.Text.Json.JsonSerializer.Serialize(notification);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending webhook notification for {Uuid} with status {Status}", uuid, status);

            var response = await _httpClient.PostAsync(webhookUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Webhook notification sent successfully for {Uuid}", uuid);
                return true;
            }

            _logger.LogWarning("Webhook notification failed for {Uuid}. Status: {StatusCode}, Response: {Response}", 
                uuid, response.StatusCode, responseContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook notification for {Uuid}", uuid);
            return false;
        }
    }

    public async Task<bool> SendBatchDeliveryStatusNotificationsAsync(IEnumerable<DeliveryStatusNotification> notifications)
    {
        try
        {
            var webhookUrl = GetWebhookUrl();
            if (string.IsNullOrEmpty(webhookUrl))
            {
                _logger.LogError("Webhook URL not configured");
                return false;
            }

            var batchPayload = new
            {
                notifications = notifications.ToList(),
                batch_size = notifications.Count(),
                timestamp = DateTime.UtcNow
            };

            var json = System.Text.Json.JsonSerializer.Serialize(batchPayload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending batch webhook notification for {Count} messages", notifications.Count());

            var response = await _httpClient.PostAsync(webhookUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Batch webhook notification sent successfully");
                return true;
            }

            _logger.LogWarning("Batch webhook notification failed. Status: {StatusCode}, Response: {Response}", 
                response.StatusCode, responseContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send batch webhook notification");
            return false;
        }
    }

    public string GetWebhookUrl()
    {
        return _configuration["WebEngage:WebhookUrl"] ?? string.Empty;
    }

    public async Task<bool> ValidateWebhookConfigurationAsync()
    {
        try
        {
            var webhookUrl = GetWebhookUrl();
            if (string.IsNullOrEmpty(webhookUrl))
            {
                _logger.LogError("Webhook URL not configured");
                return false;
            }

            // Test the webhook endpoint with a simple ping
            var pingPayload = new
            {
                type = "ping",
                timestamp = DateTime.UtcNow,
                message = "Webhook configuration test"
            };

            var json = System.Text.Json.JsonSerializer.Serialize(pingPayload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Webhook configuration validation successful");
                return true;
            }

            _logger.LogWarning("Webhook configuration validation failed. Status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook configuration validation failed");
            return false;
        }
    }
}
