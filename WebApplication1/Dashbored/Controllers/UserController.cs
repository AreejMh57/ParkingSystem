using Application.DTOs;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace Dashboard.Controllers
{
    [Authorize(Roles = "Admin")] // تأمين المتحكم للمسؤولين فقط
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // GET: /User
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Admin accessed the user management page.");

            var users = await _userService.GetAllUsersAsync();

            return View(users);
        }

        // GET: /User/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            _logger.LogInformation("Admin accessed details for user ID: {UserId}.", id);

            var user = await _userService.GetUserProfileAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User details for ID {UserId} not found.", id);
                return NotFound();
            }

            return View(user);
        }

        // GET: /User/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userService.GetUserProfileAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Attempted to edit user with ID '{UserId}', but user not found.", id);
                return NotFound();
            }

            var updateDto = new UpdateUserDto
            {
                UserId = user.Id,
               // Email = user.Email
            };

            return View(updateDto);
        }

        // POST: /User/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUserDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.UpdateUserProfileAsync(dto.UserId, dto);
                    _logger.LogInformation("User ID '{UserId}' was updated successfully.", dto.UserId);
                    return RedirectToAction(nameof(Details), new { id = dto.UserId });
                }
                catch (KeyNotFoundException)
                {
                    _logger.LogWarning("Failed to update user with ID '{UserId}'. User not found.", dto.UserId);
                    return NotFound();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user with ID '{UserId}': {Message}", dto.UserId, ex.Message);
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the user.");
                }
            }
            return View(dto);
        }

        // POST: /User/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User with ID '{UserId}' was deleted successfully.", id);
                }
                else
                {
                    var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to delete user with ID '{UserId}': {Errors}", id, errors);
                    ModelState.AddModelError(string.Empty, $"Failed to delete user: {errors}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID '{UserId}': {Message}", id, ex.Message);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while deleting the user.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}