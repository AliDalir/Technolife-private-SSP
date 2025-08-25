using System.Text.Json.Serialization;

namespace PrivateSSP.Core.Models;

/// <summary>
/// Represents an SMS request from WebEngage
/// </summary>
public class SmsRequest
{
    /// <summary>
    /// Unique identifier for the SMS request
    /// </summary>
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    /// <summary>
    /// Hashed phone number (encrypted PII)
    /// </summary>
    [JsonPropertyName("hashed_phone")]
    public string HashedPhone { get; set; } = string.Empty;

    /// <summary>
    /// SMS message content
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Sender ID for the SMS
    /// </summary>
    [JsonPropertyName("sender_id")]
    public string? SenderId { get; set; }

    /// <summary>
    /// Campaign ID from WebEngage
    /// </summary>
    [JsonPropertyName("campaign_id")]
    public string? CampaignId { get; set; }

    /// <summary>
    /// User ID from WebEngage
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    /// <summary>
    /// Additional custom data
    /// </summary>
    [JsonPropertyName("custom_data")]
    public Dictionary<string, object>? CustomData { get; set; }

    /// <summary>
    /// Timestamp when the request was received
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
