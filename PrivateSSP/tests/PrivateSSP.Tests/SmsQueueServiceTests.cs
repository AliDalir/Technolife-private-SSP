using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PrivateSSP.Core.Models;
using PrivateSSP.Services.Services;
using Xunit;

namespace PrivateSSP.Tests;

public class SmsQueueServiceTests
{
    private readonly Mock<ILogger<SmsQueueService>> _mockLogger;
    private readonly SmsQueueService _queueService;

    public SmsQueueServiceTests()
    {
        _mockLogger = new Mock<ILogger<SmsQueueService>>();
        _queueService = new SmsQueueService(_mockLogger.Object);
    }

    [Fact]
    public async Task EnqueueMessage_ValidMessage_ShouldReturnTrue()
    {
        // Arrange
        var message = new SmsMessage
        {
            Uuid = "test-uuid",
            PhoneNumber = "+1234567890",
            Message = "Test message",
            Priority = 1
        };

        // Act
        var result = await _queueService.EnqueueMessageAsync(message);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetQueueSize_AfterEnqueue_ShouldReturnOne()
    {
        // Arrange
        var message = new SmsMessage
        {
            Uuid = "test-uuid",
            PhoneNumber = "+1234567890",
            Message = "Test message",
            Priority = 1
        };

        await _queueService.EnqueueMessageAsync(message);

        // Act
        var size = await _queueService.GetQueueSizeAsync();

        // Assert
        size.Should().Be(1);
    }
}
