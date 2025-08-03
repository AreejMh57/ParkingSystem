using Microsoft.AspNetCore.Http;
using Application.DTOs;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Peresentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        
        
        [HttpPost("register")]
        public async Task<string> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for registration attempt by {Email}.", dto?.Email);
                return "BadRequest";
      
            }

            try
            {
                // التغيير: إرجاع كائن يحتوي على التوكن بدل السلسلة النصية
                var response = await _authService.CreateAccountAsync(dto);

                if (response == null)
                {
                    return ("Failed to create account");
                }

                return (
                
                    response
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during registration");
                return "Internal server error";
            }
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] loginDto dto) // التغيير: LoginDto بدل loginDto
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.SignInAsync(dto);

            if (response == null)
            {
                return Unauthorized(new { Message = "Invalid credentials" });
            }

            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpGet("me")]
        [Authorize] // التغيير: تفعيل المصادقة
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // التغيير: إرجاع كائن منظم بدل البيانات الخام
            return Ok(new
            {
                Id = userId,
                Email = userEmail
            //    Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            });
        }
    }
}