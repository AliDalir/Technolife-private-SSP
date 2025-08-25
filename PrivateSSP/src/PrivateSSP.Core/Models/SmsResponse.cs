using System.Text.Json.Serialization;

namespace PrivateSSP.Core.Models;

/// <summary>
/// Represents the response sent back to WebEngage
/// </summary>
public class SmsResponse
{
    /// <summary>
    /// Status of the SMS request
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Status code for the response
    /// </summary>
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    /// <summary>
    /// Message describing the status
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for the SMS request
    /// </summary>
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the response was sent
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional metadata about the response
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Predefined SMS response statuses
/// </summary>
public static class SmsResponseStatus
{
    public const string Success = "sms_accepted";
    public const string Rejected = "sms_rejected";
    public const string Queued = "sms_queued";
    public const string Failed = "sms_failed";
}
