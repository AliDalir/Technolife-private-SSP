namespace PrivateSSP.Core.Models;

/// <summary>
/// Represents a decrypted SMS message ready for sending
/// </summary>
public class SmsMessage
{
    /// <summary>
    /// Unique identifier for the SMS
    /// </summary>
    public string Uuid { get; set; } = string.Empty;

    /// <summary>
    /// Decrypted phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// SMS message content
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Sender ID for the SMS
    /// </summary>
    public string? SenderId { get; set; }

    /// <summary>
    /// Campaign ID from WebEngage
    /// </summary>
    public string? CampaignId { get; set; }

    /// <summary>
    /// User ID from WebEngage
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Priority of the message (1-5, where 1 is highest)
    /// </summary>
    public int Priority { get; set; } = 3;

    /// <summary>
    /// When the message should be sent
    /// </summary>
    public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Maximum retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Current retry attempt
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Additional custom data
    /// </summary>
    public Dictionary<string, object>? CustomData { get; set; }

    /// <summary>
    /// Timestamp when the message was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Status of the message
    /// </summary>
    public SmsMessageStatus Status { get; set; } = SmsMessageStatus.Pending;
}

/// <summary>
/// Status of an SMS message
/// </summary>
public enum SmsMessageStatus
{
    Pending,
    Queued,
    Sending,
    Sent,
    Delivered,
    Failed,
    Cancelled
}
