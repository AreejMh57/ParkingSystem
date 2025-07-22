// Presentation/Controllers/NotificationController.cs
using Application.DTOs; // لـNotification DTOs
using Application.IServices; // لـINotificationService
using Microsoft.AspNetCore.Authorization; // لـ[Authorize] attribute
using Microsoft.AspNetCore.Mvc; // لـControllerBase, IActionResult, إلخ
using System; // لـGuid
using System.Collections.Generic; // لـIEnumerable
using System.Security.Claims; // لـClaimTypes.NameIdentifier
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // لـIdentityResult

namespace Presentation.Controllers
{
    [ApiController] // يشير إلى أن هذا المتحكم يستجيب لطلبات API الويب
    [Route("api/[controller]")] // يحدد المسار الأساسي لهذا المتحكم (مثلاً: /api/Notification)
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Creates and records a new notification for a user.
        /// Requires 'notification_create' permission. (Typically for admin/system use)
        /// </summary>
        /// <param name="dto">Notification creation details.</param>
        /// <returns>The created NotificationDto on success.</returns>
        [HttpPost("create")]
        [Authorize(Policy = "notification_create")] // تتطلب صلاحية إنشاء إشعارات (غالباً للمسؤولين)
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // إذا لم يتم تحديد UserId في الـDTO (مثلاً إذا كان الإشعار لـauthenticated user)
                // string authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // dto.UserId = dto.UserId ?? authenticatedUserId; // استخدم الـUserId من الـDTO أو من التوكن

                var newNotification = await _notificationService.CreateNotificationAsync(dto);
                return StatusCode(201, newNotification); // HTTP 201 Created
            }
            catch (KeyNotFoundException ex) // إذا كان المستخدم (UserId) غير موجود
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the notification.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all notifications for the authenticated user.
        /// Requires 'notification_browse' permission.
        /// </summary>
        /// <returns>A list of NotificationDto.</returns>
        [HttpGet("my-notifications")]
        [Authorize(Policy = "notification_browse")] // تتطلب صلاحية تصفح الإشعارات
        public async Task<IActionResult> GetMyNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token. Please log in.");
            }
            try
            {
                var notifications = await _notificationService.GetUserNotificationsAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving notifications.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves only unread notifications for the authenticated user.
        /// Requires 'notification_browse' permission.
        /// </summary>
        /// <returns>A list of unread NotificationDto.</returns>
        [HttpGet("my-notifications/unread")]
        [Authorize(Policy = "notification_browse")] // تتطلب صلاحية تصفح الإشعارات
        public async Task<IActionResult> GetMyUnreadNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token. Please log in.");
            }
            try
            {
                var notifications = await _notificationService.GetUserUnreadNotificationsAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving unread notifications.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Marks a specific notification as read.
        /// Requires 'notification_update' permission.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to mark as read.</param>
        /// <returns>The updated NotificationDto.</returns>
        [HttpPut("{notificationId}/mark-as-read")]
        [Authorize(Policy = "notification_update")] // تتطلب صلاحية تعديل الإشعارات
        public async Task<IActionResult> MarkNotificationAsRead(Guid notificationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token. Please log in.");
            }
            try
            {
                var updatedNotification = await _notificationService.MarkNotificationAsReadAsync(notificationId, userId);
                return Ok(updatedNotification);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex) // إذا المستخدم غير مخول
            {
                return Forbid(ex.Message); // HTTP 403 Forbidden
            }
            catch (InvalidOperationException ex) // إذا كان الإشعار مقروءاً بالفعل
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while marking the notification as read.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a specific notification.
        /// Requires 'notification_delete' permission.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to delete.</param>
        /// <returns>Success message.</returns>
        [HttpDelete("{notificationId}")]
        [Authorize(Policy = "notification_delete")] // تتطلب صلاحية حذف الإشعارات
        public async Task<IActionResult> DeleteNotification(Guid notificationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token. Please log in.");
            }
            try
            {
                var result = await _notificationService.DeleteNotificationAsync(notificationId, userId);
                if (result.Succeeded)
                {
                    return NoContent(); // HTTP 204 No Content for successful deletion
                }
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Message = "Failed to delete notification.", Errors = errors });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex) // إذا المستخدم غير مخول
            {
                return Forbid(ex.Message); // HTTP 403 Forbidden
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the notification.", Details = ex.Message });
            }
        }
    }
}