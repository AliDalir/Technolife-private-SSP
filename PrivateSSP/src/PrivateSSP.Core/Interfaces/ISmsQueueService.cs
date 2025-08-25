using PrivateSSP.Core.Models;

namespace PrivateSSP.Core.Interfaces;

/// <summary>
/// Service for managing SMS message queuing and processing
/// </summary>
public interface ISmsQueueService
{
    /// <summary>
    /// Adds a message to the queue
    /// </summary>
    /// <param name="message">The SMS message to queue</param>
    /// <returns>True if the message was successfully queued</returns>
    Task<bool> EnqueueMessageAsync(SmsMessage message);

    /// <summary>
    /// Gets the next message from the queue
    /// </summary>
    /// <returns>The next SMS message to process</returns>
    Task<SmsMessage?> DequeueMessageAsync();

    /// <summary>
    /// Gets the current queue size
    /// </summary>
    /// <returns>The number of messages in the queue</returns>
    Task<int> GetQueueSizeAsync();

    /// <summary>
    /// Removes a message from the queue
    /// </summary>
    /// <param name="uuid">The UUID of the message to remove</param>
    /// <returns>True if the message was successfully removed</returns>
    Task<bool> RemoveMessageAsync(string uuid);

    /// <summary>
    /// Gets all messages in the queue
    /// </summary>
    /// <returns>List of all queued messages</returns>
    Task<IEnumerable<SmsMessage>> GetAllQueuedMessagesAsync();

    /// <summary>
    /// Clears all messages from the queue
    /// </summary>
    /// <returns>True if the queue was successfully cleared</returns>
    Task<bool> ClearQueueAsync();
}
