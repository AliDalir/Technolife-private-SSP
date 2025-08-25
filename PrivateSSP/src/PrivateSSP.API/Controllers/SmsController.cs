using Microsoft.AspNetCore.Mvc;
using PrivateSSP.Core.Models;
using PrivateSSP.Services.Services;

namespace PrivateSSP.API.Controllers;

/// <summary>
/// Controller for handling SMS requests from WebEngage
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SmsController : ControllerBase
{
    private readonly ILogger<SmsController> _logger;
    private readonly SmsProcessingService _smsProcessingService;

    public SmsController(ILogger<SmsController> logger, SmsProcessingService smsProcessingService)
    {
        _logger = logger;
        _smsProcessingService = smsProcessingService;
    }

    /// <summary>
    /// Receives SMS requests from WebEngage
    /// </summary>
    /// <param name="request">The SMS request from WebEngage</param>
    /// <returns>Response indicating the status of the request</returns>
    [HttpPost("send")]
    public async Task<ActionResult<SmsResponse>> SendSms([FromBody] SmsRequest request)
    {
        try
        {
            _logger.LogInformation("Received SMS request {Uuid} from WebEngage", request.Uuid);

            // Validate request
            if (request == null)
            {
                return BadRequest(new SmsResponse
                {
                    Status = SmsResponseStatus.Rejected,
                    StatusCode = 2001,
                    Message = "Request body is required",
                    Uuid = string.Empty
                });
            }

            if (string.IsNullOrWhiteSpace(request.Uuid))
            {
                return BadRequest(new SmsResponse
                {
                    Status = SmsResponseStatus.Rejected,
                    StatusCode = 2001,
                    Message = "UUID is required",
                    Uuid = string.Empty
                });
            }

            // Process the SMS request
            var response = await _smsProcessingService.ProcessSmsRequestAsync(request);

            // Return appropriate HTTP status based on response
            if (response.Status == SmsResponseStatus.Success)
            {
                return Ok(response);
            }
            else if (response.StatusCode == 2015) // Throttling error
            {
                return StatusCode(429, response);
            }
            else
            {
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing SMS request {Uuid}", request?.Uuid);
            
            var errorResponse = new SmsResponse
            {
                Status = SmsResponseStatus.Rejected,
                StatusCode = 9988,
                Message = "Internal server error",
                Uuid = request?.Uuid ?? string.Empty
            };

            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    public ActionResult<object> Health()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "PrivateSSP",
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Gets the current queue status
    /// </summary>
    /// <returns>Queue status information</returns>
    [HttpGet("queue/status")]
    public async Task<ActionResult<object>> GetQueueStatus()
    {
        try
        {
            // This would require injecting the queue service directly
            // For now, return a placeholder response
            return Ok(new
            {
                status = "operational",
                timestamp = DateTime.UtcNow,
                queue_size = "N/A - requires direct queue service access"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queue status");
            return StatusCode(500, new { error = "Failed to get queue status" });
        }
    }
}
