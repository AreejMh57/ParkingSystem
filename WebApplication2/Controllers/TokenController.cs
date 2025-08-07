// Presentation/Controllers/TokenController.cs
using Application.DTOs; 
using Application.IServices; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 
using System; 
using System.Collections.Generic; 
using System.Security.Claims; 
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; 

namespace Presentation.Controllers
{
    [ApiController] // يشير إلى أن هذا المتحكم يستجيب لطلبات API الويب
    [Route("api/[controller]")] // يحدد المسار الأساسي لهذا المتحكم (مثلاً: /api/Token)
    // <--- حماية عامة: يمكن أن تكون لـAdmin أو ParkingManager إذا كانت الإدارة مركزية --->
   //فر
   //[Authorize(Roles = "Admin,ParkingManager")] // هذا المتحكم خاص بالإدارة بشكل عام
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }



        /// <summary>
        /// Requests a specific sensor access token for an authenticated user and booking.
        /// This acts as the "Service Ticket Request" in Kerberos-like flow.
        /// Requires 'token_create' permission. (Or a more specific 'sensor_access_token_request' if defined).
        /// </summary>
        /// <param name="dto">Details for the token request (UserId, BookingId, ExpirationMinutes).</param>
        /// <returns>A TokenDto of the newly generated sensor access token.</returns>
        [HttpPost("request-sensor-access")]
        [Authorize(Policy = "TOKEN_CREATE")] // <--- صلاحية إنشاء توكن (Admin, ParkingManager, Customer)
        public async Task<IActionResult> RequestSensorAccessToken([FromBody] CreateTokenDto dto)
        {
            // 1. استخراج UserId من توكن المستخدم المصادق عليه (الأكثر أماناً)
            var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(authenticatedUserId))
            {
                return Unauthorized("User ID not found in token. Please log in.");
            }

            // 2. التحقق من أن الطلب يخص المستخدم نفسه (إذا لم يكن Admin)
            // (هذا ضروري إذا كان Customer هو من يطلب التوكن لنفسه)
            // if (dto.UserId != authenticatedUserId) // إذا كان UserId في DTO مختلفاً عن المصادق عليه
            // {
            //     // Optional: Check if the authenticated user is an Admin
            //     // if (!User.IsInRole("Admin")) // إذا لم يكن Admin، فليست له صلاحية لطلب توكن لغيره
            //     // {
            //     //     return Forbid("You can only request an access token for yourself.");
            //     // }
            //     // else { dto.UserId = authenticatedUserId; } // إذا كان Admin، دع له يطلب لغيره
            // }

            // بما أن UserId مطلوب في CreateTokenDto، سنعيد تعيينه من التوكن لضمان الاتساق والأمان
            dto.UserId = authenticatedUserId;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var sensorAccessToken = await _tokenService.CreateCustomTokenAsync(dto);
                return StatusCode(201, sensorAccessToken); // HTTP 201 Created
            }
            catch (KeyNotFoundException ex) // إذا كان المستخدم أو الحجز غير موجود
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while requesting sensor access token.", Details = ex.Message });
            }
        }
    
        /// <summary>
        /// Creates and stores a new custom token for a specific user and booking
        /// Requires 'token_create' permission.
        /// </summary>
        /// <param name="dto">DTO containing token creation details (UserId, ExpirationMinutes, BookingId).</param>
        /// <returns>A TokenDto of the newly created token.</returns>
        [HttpPost("create")]
        [Authorize(Policy = "TOKEN_CREATE")] // تتطلب صلاحية إنشاء توكنات
        public async Task<IActionResult> CreateToken([FromBody] CreateTokenDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // إذا لم يتم تحديد UserId في DTO، يمكن أخذه من المستخدم المصادق عليه
                // string authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // dto.UserId = dto.UserId ?? authenticatedUserId; 

                var newToken = await _tokenService.CreateCustomTokenAsync(dto);
                return StatusCode(201, newToken); // HTTP 201 Created
            }
            catch (KeyNotFoundException ex) // إذا كان المستخدم (UserId) أو الحجز (BookingId) غير موجود
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the token.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Validates a submitted token for a specific user.
        /// This endpoint might be [AllowAnonymous] if validation happens externally (e.g., via email link).
        /// </summary>
        /// <param name="dto">DTO containing user ID and the token string.</param>
        /// <returns>Success message or detailed failure reasons.</returns>
         // يمكن جعله متاحاً للعامة إذا كانت عملية التحقق لا تتطلب تسجيل دخول
                         //[Authorize(Policy = "TOKEN_VALIDATE")] // تتطلب صلاحية التحقق من التوكنات (إذا كان للمصادقين)
        /*public async Task<IActionResult> ValidateToken([FromBody] ValidateBookingTokenDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _tokenService.ValidateCustomTokenAsync(dto);
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
        */
        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> ValidateToken([FromBody] ValidateBookingTokenDto dto)
        {
            try
            {
              //  _logger.LogInformation("Received token validation request for User {UserId}, Booking {BookingId}.", dto.UserId, dto.BookingId);

                // استدعاء خدمة التوكن للتحقق من صحة التوكن
                var validationResult = await _tokenService.ValidateBookingTokenAsync(dto);

                // إذا كانت النتيجة تحتوي على رسالة خطأ، أعدها كـ BadRequest
                if (validationResult.Contains("Invalid token") ||
                    validationResult.Contains("Token has expired") ||
                    validationResult.Contains("Token has already been used"))
                {
                //    _logger.LogWarning("Token validation failed: {ValidationResult}", validationResult);
                    return BadRequest(validationResult);
                }

                // إذا كان التوكن صالحًا، أعد النتيجة بنجاح (OK)
            //    _logger.LogInformation("Token validation successful: {ValidationResult}", validationResult);
                return Ok(validationResult);
            }
            catch (Exception ex)
            {
                // في حالة وجود أي خطأ غير متوقع، يعود برمز حالة 500 Internal Server Error.
              //  _logger.LogError(ex, "An unexpected error occurred during token validation for User {UserId}, Booking {BookingId}.", dto.UserId, dto.BookingId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        /// <summary>
        /// Retrieves active tokens for a user by their ID and/or BookingId (e.g., for admin review).
        /// Requires 'token_browse' permission.
        /// </summary>
        /// <param name="userId">The ID of the user whose tokens are to be retrieved.</param>
        /// <param name="bookingId">The ID of the booking (optional filter).</param>
        /// <returns>A list of TokenDto.</returns>
        [HttpGet("active-by-user")]
        [Authorize(Policy = "TOKEN_BROWSE")] // تتطلب صلاحية تصفح التوكنات
        public async Task<IActionResult> GetActiveTokensByUserId([FromQuery] string userId, [FromQuery] Guid? bookingId = null)
        {
            try
            {
                // يمكن إضافة تحقق هنا: هل المستخدم المصادق عليه له صلاحية رؤية توكنات مستخدم آخر؟
                // مثلاً: إذا لم يكن Admin، يجب أن يكون userId هو User.FindFirstValue(ClaimTypes.NameIdentifier)
                var tokens = await _tokenService.GetActiveTokensByUserIdAndBookingIdAsync(userId, bookingId);
                return Ok(tokens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving active tokens.", Details = ex.Message });
            }
        }

    
        // Cleans up expired and used tokens from the database. (Admin-level trigger)
       
        [HttpDelete("cleanup-expired-used")]
        [Authorize(Policy = "TOKEN_DELETE")] ////Requires permission to delete tokens
        public async Task<IActionResult> CleanupExpiredAndUsedTokens()
        {
            try
            {
                var count = await _tokenService.CleanupExpiredTokensAsync();
                return Ok(new { Message = $"Cleaned up {count} expired or used tokens." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred during token cleanup.", Details = ex.Message });
            }
        }
    }
}