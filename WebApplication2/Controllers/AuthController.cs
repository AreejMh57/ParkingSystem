using Microsoft.AspNetCore.Http;
using Application.DTOs; // For LoginDto, RegisterDto, LoginResponseDto
using Application.IServices; // For IAuthService
using Microsoft.AspNetCore.Authorization; // For [Authorize] attribute
using Microsoft.AspNetCore.Mvc; // For ControllerBase, IActionResult, etc.
using System.Linq; // For Select, Join
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // For IdentityResult
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="dto">User registration details (UserName, Email, Password, Role).</param>

        /// <summary>
        /// يسجل مستخدمًا جديدًا في النظام ويُرجع توكن JWT (API Endpoint).
        /// </summary>
        /// <param name="dto">تفاصيل تسجيل المستخدم (Email, Password, Role).</param>
        /// <returns>JSON يحتوي على رسالة نجاح وتوكن عند النجاح، أو أخطاء مفصلة.</returns>

        [HttpPost("register")] // المسار الكامل سيكون /api/Account/register
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for registration attempt by {Email}.", dto?.Email);
                return BadRequest(ModelState);
            }

            try
            {
                // استدعاء خدمة إنشاء الحساب التي ترجع string بالصيغة المطلوبة
                var resultString = await _authService.CreateAccountAsync(dto);

                if (resultString == null)
                {
                    _logger.LogWarning("Account creation failed for {Email} with no specific error details returned.", dto.Email);
                    return BadRequest("Failed to create account. Please check the provided details.");
                }

                _logger.LogInformation("Registration successful for {Email}. Details: {ResultString}", dto.Email, resultString);
                // إرجاع الـstring الذي يحتوي على الـIDs والـtoken
                return Ok(resultString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled error occurred during registration for {Email}.", dto.Email);
                return StatusCode(500, "An internal server error occurred during registration. Please try again later.");
            }
        }
        /*
       [HttpPost("register")]
       [AllowAnonymous] // Allows unauthenticated users to access this endpoint
       public async Task<IActionResult> Register([FromBody] RegisterDto dto)
       {
           // Basic model validation
           if (!ModelState.IsValid)
           {
               return BadRequest(ModelState);
           }

           var result = await _authService.CreateAccountAsync(dto);

           if (result.Succeeded)
           {
               // Optionally, you could automatically log in the user here and return a token
               // For now, we'll just return a success message.
               return Ok(new { Message = "User registered successfully. Please log in." });
           }
           else
           {
               // Extract and return detailed errors from IdentityResult
               var errors = result.Errors.Select(e => e.Description);
               return BadRequest(new { Message = "User registration failed.", Errors = errors });
           }
       }*/

        /// <summary>
        /// Authenticates a user and returns a JWT token upon successful login.
        /// </summary>
        /// <param name="dto">User login credentials (Email, Password, RememberMe).</param>
        /// <returns>LoginResponseDto containing JWT token and user info on success.</returns>
        [HttpPost("login")]
        [AllowAnonymous] // Allows unauthenticated users to access this endpoint
        public async Task<IActionResult> Login([FromBody] loginDto dto)
        {


            //_logger.LogInformation("Register request received for email: {Email}", dto.Email); // استخدام الـlogger
            // Basic model validation
            if (!ModelState.IsValid)
            {
                // _logger.LogWarning("Register request for email: {Email} failed due to invalid model state.", dto.Email); // تسجيل تحذير
                return BadRequest(ModelState);
            }

            var response = await _authService.SignInAsync(dto);

            if (response == null) // This indicates login failure (user not found or bad credentials)
            {
                _logger.LogWarning("Login failed for email: {Email}. Invalid credentials.", dto.Email);
                return Unauthorized(new { Message = "Invalid login credentials." });
            }

            // Login successful, return the token and user info
            return Ok(response);
        }
        /// <summary>
        /// Logs out the current authenticated user.
        /// </summary>
        /// <returns>Success message.</returns>
        [HttpPost("logout")]
        [Authorize] // Requires the user to be authenticated to logout
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return Ok(new { Message = "Logged out successfully." });
        }

        // Example: Get current user info (requires authentication)
        /// <summary>
        /// Gets the profile of the currently authenticated user.
        /// Requires authentication.
        /// </summary>
        /// <returns>UserDto of the current user.</returns>
      /*  [HttpGet("me")] // <--- مسار نقطة النهاية
       // [Authorize] // <--- هذه هي التي تتحقق من التوكن وتوفر معلومات المستخدم
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            // استخراج معرف المستخدم (ID) من الـClaims في التوكن الذي تم التحقق منه
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // استخراج البريد الإلكتروني (Email) من الـClaims في التوكن
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            // استخراج اسم المستخدم (UserName) من الـClaims في التوكن
            var userName = User.FindFirstValue(ClaimTypes.Name);

            // التحقق إذا كان معرف المستخدم فارغاً (لا ينبغي أن يحدث في طلب مصادق عليه)
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("API GetCurrentUserProfile: User ID not found in token for an authorized request.");
                return Unauthorized("User ID not found in token.");
            }

            _logger.LogInformation("API GetCurrentUserProfile request received for user ID: {UserId}", userId);

            // إرجاع المعلومات المطلوبة (Id, Email, UserName)
            return Ok(new
            {
                Id = userId,
                Email = userEmail,
            
                // ❌ ملاحظة: لا تُرجع PasswordHash أبداً لأسباب أمنية.
                // يمكنك إضافة أدوار المستخدم إذا أردت:
                // Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            });
        }*/
        [HttpGet("me")]
     //   [Authorize] // Requires authentication
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }
            
            // Assuming you have a UserService to get user profile
            // private readonly IUserService _userService; (inject this)
            // var userProfile = await _userService.GetUserProfileAsync(userId);
            // if (userProfile == null) return NotFound("User profile not found.");
            // return Ok(userProfile);

            // For now, just return basic info from claims if UserService is not ready
            return Ok(new
            {
                Id = userId,
             
         
                Email = User.FindFirstValue(ClaimTypes.Email),
                Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
            });
        }
    }
}

