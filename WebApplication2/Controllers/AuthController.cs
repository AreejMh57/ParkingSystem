using Microsoft.AspNetCore.Http;
using Application.DTOs; // For LoginDto, RegisterDto, LoginResponseDto
using Application.IServices; // For IAuthService
using Microsoft.AspNetCore.Authorization; // For [Authorize] attribute
using Microsoft.AspNetCore.Mvc; // For ControllerBase, IActionResult, etc.
using System.Linq; // For Select, Join
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // For IdentityResult
using System.Security.Claims;
namespace Peresentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="dto">User registration details (UserName, Email, Password, Role).</param>
        /// <returns>Success message or detailed errors.</returns>
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
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token upon successful login.
        /// </summary>
        /// <param name="dto">User login credentials (Email, Password, RememberMe).</param>
        /// <returns>LoginResponseDto containing JWT token and user info on success.</returns>
        [HttpPost("login")]
        [AllowAnonymous] // Allows unauthenticated users to access this endpoint
        public async Task<IActionResult> Login([FromBody] loginDto dto)
        {
            // Basic model validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.SignInAsync(dto);

            if (response == null) // This indicates login failure (user not found or bad credentials)
            {
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
        [HttpGet("me")]
        [Authorize] // Requires authentication
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

