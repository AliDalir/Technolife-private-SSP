using PrivateSSP.Core.Models;

namespace PrivateSSP.Core.Interfaces;

/// <summary>
/// Service for sending SMS messages through various providers
/// </summary>
public interface ISmsProviderService
{
    /// <summary>
    /// Sends an SMS message
    /// </summary>
    /// <param name="message">The SMS message to send</param>
    /// <returns>The result of the SMS sending operation</returns>
    Task<SmsSendResult> SendSmsAsync(SmsMessage message);

    /// <summary>
    /// Gets the delivery status of an SMS
    /// </summary>
    /// <param name="messageId">The message ID from the provider</param>
    /// <returns>The delivery status</returns>
    Task<SmsDeliveryStatus> GetDeliveryStatusAsync(string messageId);

    /// <summary>
    /// Gets the balance/credits available with the provider
    /// </summary>
    /// <returns>The available balance</returns>
    Task<decimal> GetBalanceAsync();

    /// <summary>
    /// Validates a phone number format
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate</param>
    /// <returns>True if the phone number is valid</returns>
    Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
}

/// <summary>
/// Result of an SMS sending operation
/// </summary>
public class SmsSendResult
{
    public bool IsSuccess { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public SmsDeliveryStatus Status { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Delivery status of an SMS
/// </summary>
public enum SmsDeliveryStatus
{
    Pending,
    Sent,
    Delivered,
    Failed,
    Rejected,
    Queued
}
