using PrivateSSP.Core.Models;

namespace PrivateSSP.Core.Interfaces;

/// <summary>
/// Service for sending webhook notifications to WebEngage
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Sends a delivery status notification to WebEngage
    /// </summary>
    /// <param name="uuid">The UUID of the SMS message</param>
    /// <param name="status">The delivery status</param>
    /// <param name="statusCode">The status code</param>
    /// <param name="message">Additional message details</param>
    /// <returns>True if the webhook was sent successfully</returns>
    Task<bool> SendDeliveryStatusNotificationAsync(string uuid, string status, int statusCode, string message);

    /// <summary>
    /// Sends a batch of delivery status notifications
    /// </summary>
    /// <param name="notifications">List of notifications to send</param>
    /// <returns>True if all webhooks were sent successfully</returns>
    Task<bool> SendBatchDeliveryStatusNotificationsAsync(IEnumerable<DeliveryStatusNotification> notifications);

    /// <summary>
    /// Gets the webhook URL for WebEngage
    /// </summary>
    /// <returns>The webhook URL</returns>
    string GetWebhookUrl();

    /// <summary>
    /// Validates the webhook configuration
    /// </summary>
    /// <returns>True if the webhook is properly configured</returns>
    Task<bool> ValidateWebhookConfigurationAsync();
}

/// <summary>
/// Represents a delivery status notification
/// </summary>
public class DeliveryStatusNotification
{
    public string Uuid { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }
}
