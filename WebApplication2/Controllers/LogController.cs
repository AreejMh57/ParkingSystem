using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Application.IServices;
using Application.DTOs.Log; // Assuming CreateLogRequest is in this namespace or adjust as needed

namespace YourProjectName.Controllers // Replace YourProjectName with your actual project's namespace for controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogController(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="request">The log message request.</param>
        /// <returns>A 202 Accepted response if the log request is processed.</returns>
        [HttpPost("info")]
        [ProducesResponseType(202)] // Accepted
        [ProducesResponseType(400)] // Bad Request if validation fails
        [ProducesResponseType(500)] // Internal Server Error for unhandled exceptions
        public async Task<IActionResult> LogInfo([FromBody] CreateLogRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // The service handles its own exceptions internally and writes to console.
                // For an API, we generally don't want to expose internal logging client failures directly to the client
                // unless it's critical for the client to know.
                // Since the service methods return Task and handle exceptions internally (Console.WriteLine),
                // we'll just await and return Accepted.
                await _logService.LogInfoAsync(request.Message);
                return Accepted(); // 202 Accepted: The request has been accepted for processing, but the processing has not been completed.
            }
            catch (Exception ex)
            {
                // In a real application, you'd log this ex using your main application logger (e.g., Serilog, NLog)
                // not just Console.WriteLine.
                Console.WriteLine($"Error in LogController.LogInfo: {ex.Message}");
                return StatusCode(500, "An error occurred while attempting to log an informational message.");
            }
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="request">The log message request.</param>
        /// <returns>A 202 Accepted response if the log request is processed.</returns>
        [HttpPost("warning")]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> LogWarning([FromBody] CreateLogRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _logService.LogWarningAsync(request.Message);
                return Accepted();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LogController.LogWarning: {ex.Message}");
                return StatusCode(500, "An error occurred while attempting to log a warning message.");
            }
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="request">The log message request.</param>
        /// <returns>A 202 Accepted response if the log request is processed.</returns>
        [HttpPost("error")]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> LogError([FromBody] CreateLogRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _logService.LogErrorAsync(request.Message);
                return Accepted();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LogController.LogError: {ex.Message}");
                return StatusCode(500, "An error occurred while attempting to log an error message.");
            }
        }
    }
}