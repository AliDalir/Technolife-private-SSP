using Microsoft.Extensions.Logging;
using PrivateSSP.Core.Interfaces;
using PrivateSSP.Core.Models;
using System.Collections.Concurrent;

namespace PrivateSSP.Services.Services;

/// <summary>
/// Service for managing SMS message queuing and processing
/// </summary>
public class SmsQueueService : ISmsQueueService
{
    private readonly ILogger<SmsQueueService> _logger;
    private readonly ConcurrentQueue<SmsMessage> _highPriorityQueue;
    private readonly ConcurrentQueue<SmsMessage> _normalPriorityQueue;
    private readonly ConcurrentQueue<SmsMessage> _lowPriorityQueue;
    private readonly ConcurrentDictionary<string, SmsMessage> _messageLookup;
    private readonly SemaphoreSlim _queueSemaphore;

    public SmsQueueService(ILogger<SmsQueueService> logger)
    {
        _logger = logger;
        _highPriorityQueue = new ConcurrentQueue<SmsMessage>();
        _normalPriorityQueue = new ConcurrentQueue<SmsMessage>();
        _lowPriorityQueue = new ConcurrentQueue<SmsMessage>();
        _messageLookup = new ConcurrentDictionary<string, SmsMessage>();
        _queueSemaphore = new SemaphoreSlim(1, 1);
    }

    public async Task<bool> EnqueueMessageAsync(SmsMessage message)
    {
        try
        {
            if (message == null)
                return false;

            await _queueSemaphore.WaitAsync();

            try
            {
                // Add to appropriate priority queue
                switch (message.Priority)
                {
                    case 1:
                    case 2:
                        _highPriorityQueue.Enqueue(message);
                        break;
                    case 3:
                        _normalPriorityQueue.Enqueue(message);
                        break;
                    case 4:
                    case 5:
                        _lowPriorityQueue.Enqueue(message);
                        break;
                    default:
                        _normalPriorityQueue.Enqueue(message);
                        break;
                }

                // Add to lookup dictionary
                _messageLookup.TryAdd(message.Uuid, message);

                _logger.LogInformation("Message {Uuid} enqueued with priority {Priority}", message.Uuid, message.Priority);
                return true;
            }
            finally
            {
                _queueSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue message {Uuid}", message?.Uuid);
            return false;
        }
    }

    public async Task<SmsMessage?> DequeueMessageAsync()
    {
        try
        {
            await _queueSemaphore.WaitAsync();

            try
            {
                // Try high priority queue first
                if (_highPriorityQueue.TryDequeue(out var highPriorityMessage))
                {
                    _messageLookup.TryRemove(highPriorityMessage.Uuid, out _);
                    _logger.LogInformation("Dequeued high priority message {Uuid}", highPriorityMessage.Uuid);
                    return highPriorityMessage;
                }

                // Try normal priority queue
                if (_normalPriorityQueue.TryDequeue(out var normalPriorityMessage))
                {
                    _messageLookup.TryRemove(normalPriorityMessage.Uuid, out _);
                    _logger.LogInformation("Dequeued normal priority message {Uuid}", normalPriorityMessage.Uuid);
                    return normalPriorityMessage;
                }

                // Try low priority queue
                if (_lowPriorityQueue.TryDequeue(out var lowPriorityMessage))
                {
                    _messageLookup.TryRemove(lowPriorityMessage.Uuid, out _);
                    _logger.LogInformation("Dequeued low priority message {Uuid}", lowPriorityMessage.Uuid);
                    return lowPriorityMessage;
                }

                return null;
            }
            finally
            {
                _queueSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dequeue message");
            return null;
        }
    }

    public async Task<int> GetQueueSizeAsync()
    {
        try
        {
            await _queueSemaphore.WaitAsync();

            try
            {
                return _highPriorityQueue.Count + _normalPriorityQueue.Count + _lowPriorityQueue.Count;
            }
            finally
            {
                _queueSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queue size");
            return 0;
        }
    }

    public async Task<bool> RemoveMessageAsync(string uuid)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(uuid))
                return false;

            await _queueSemaphore.WaitAsync();

            try
            {
                if (_messageLookup.TryRemove(uuid, out var message))
                {
                    _logger.LogInformation("Message {Uuid} removed from queue", uuid);
                    return true;
                }

                return false;
            }
            finally
            {
                _queueSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove message {Uuid}", uuid);
            return false;
        }
    }

    public async Task<IEnumerable<SmsMessage>> GetAllQueuedMessagesAsync()
    {
        try
        {
            await _queueSemaphore.WaitAsync();

            try
            {
                var allMessages = new List<SmsMessage>();
                allMessages.AddRange(_highPriorityQueue);
                allMessages.AddRange(_normalPriorityQueue);
                allMessages.AddRange(_lowPriorityQueue);

                return allMessages.OrderBy(m => m.Priority).ThenBy(m => m.CreatedAt);
            }
            finally
            {
                _queueSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all queued messages");
            return Enumerable.Empty<SmsMessage>();
        }
    }

    public async Task<bool> ClearQueueAsync()
    {
        try
        {
            await _queueSemaphore.WaitAsync();

            try
            {
                while (_highPriorityQueue.TryDequeue(out _)) { }
                while (_normalPriorityQueue.TryDequeue(out _)) { }
                while (_lowPriorityQueue.TryDequeue(out _)) { }
                _messageLookup.Clear();

                _logger.LogInformation("Queue cleared successfully");
                return true;
            }
            finally
            {
                _queueSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear queue");
            return false;
        }
    }
}
