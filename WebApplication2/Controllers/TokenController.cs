// Presentation/Controllers/TokenController.cs
using Application.DTOs; // لـToken DTOs
using Application.IServices; // لـITokenService
using Microsoft.AspNetCore.Authorization; // لـ[Authorize] attribute
using Microsoft.AspNetCore.Mvc; // لـControllerBase, IActionResult, إلخ
using System; // لـGuid
using System.Collections.Generic; // لـIEnumerable
using System.Security.Claims; // لـClaimTypes.NameIdentifier (إذا لزم)
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // لـIdentityResult

namespace Presentation.Controllers
{
    [ApiController] // يشير إلى أن هذا المتحكم يستجيب لطلبات API الويب
    [Route("api/[controller]")] // يحدد المسار الأساسي لهذا المتحكم (مثلاً: /api/Token)
    // <--- حماية عامة: يمكن أن تكون لـAdmin أو ParkingManager إذا كانت الإدارة مركزية --->
    [Authorize(Roles = "Admin,ParkingManager")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        /// <summary>
        /// Creates and stores a new token for a specific booking.
        /// Requires 'token_create' permission.
        /// </summary>
        /// <param name="dto">DTO containing booking ID and expiration minutes.</param>
        /// <returns>A TokenDto of the newly created token.</returns>
        [HttpPost("create")]
        [Authorize(Policy = "token_create")] // تتطلب صلاحية إنشاء توكنات
        public async Task<IActionResult> CreateBookingToken([FromBody] CreateTokenDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var newToken = await _tokenService.CreateBookingTokenAsync(dto);
                return StatusCode(201, newToken); // HTTP 201 Created
            }
            catch (KeyNotFoundException ex) // إذا كان الحجز (BookingId) غير موجود
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the booking token.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Validates a submitted token for a specific booking.
        /// This endpoint might be [AllowAnonymous] if validation happens externally (e.g., via email link).
        /// </summary>
        /// <param name="dto">DTO containing booking ID and the token string.</param>
        /// <returns>Success message or detailed failure reasons.</returns>
        [HttpPost("validate")]
        // [AllowAnonymous] // إذا كان هذا الـendpoint متاحاً للعامة (مثلاً للتحقق من رابط)
        [Authorize(Policy = "token_validate")] // تتطلب صلاحية التحقق من التوكنات (إذا كان للمصادقين)
        public async Task<IActionResult> ValidateBookingToken([FromBody] ValidateBookingTokenDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _tokenService.ValidateBookingTokenAsync(dto);
                if (result.Succeeded)
                {
                    return Ok(new { Message = "Token validated successfully." });
                }
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Message = "Token validation failed.", Errors = errors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred during token validation.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves active tokens for a booking (e.g., for admin review).
        /// Requires 'token_browse' permission.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>A list of TokenDto.</returns>
        [HttpGet("by-booking/{bookingId}")]
        [Authorize(Policy = "token_browse")] // تتطلب صلاحية تصفح التوكنات
        public async Task<IActionResult> GetBookingTokens(Guid bookingId)
        {
            try
            {
                var tokens = await _tokenService.GetBookingTokensAsync(bookingId);
                return Ok(tokens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving booking tokens.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Cleans up expired tokens from the database. (Admin-level trigger)
        /// Requires 'token_delete' permission.
        /// </summary>
        /// <returns>Number of tokens cleaned up.</returns>
        [HttpDelete("cleanup-expired")]
        [Authorize(Policy = "token_delete")] // تتطلب صلاحية حذف التوكنات
        public async Task<IActionResult> CleanupExpiredTokens()
        {
            try
            {
                var count = await _tokenService.CleanupExpiredTokensAsync();
                return Ok(new { Message = $"Cleaned up {count} expired tokens." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred during token cleanup.", Details = ex.Message });
            }
        }
    }
}