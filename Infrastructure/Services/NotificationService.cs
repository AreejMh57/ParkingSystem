using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.IServices;
using AutoMapper;
using Domain.Entities;
using Domain.IRepositories;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<Notification> _notificationRepo;
        private readonly ILogService _logService;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager; // To validate UserId if needed

        public NotificationService(
            IRepository<Notification> notificationRepo,
            ILogService logService,
            IMapper mapper,
            UserManager<User> userManager) // Inject UserManager
        {
            _notificationRepo = notificationRepo;
            _logService = logService;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto)
        {
            // Optional: Validate if UserId exists
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                await _logService.LogWarningAsync($"Notification creation failed: User {dto.UserId} not found.");
                throw new KeyNotFoundException($"User with ID {dto.UserId} not found.");
            }

            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = dto.UserId,
              
                Message = dto.Message,
                IsRead = false, // Notifications are unread by default
                CreatedAt = DateTime.UtcNow,
                // UpdatedAt is not typically used for notifications unless they can be edited
                Channel = dto.Channel,
                BookingId = dto.BookingId
            };

            await _notificationRepo.AddAsync(notification);
            await _notificationRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Notification {notification.NotificationId} created for user {dto.UserId}");

            return _mapper.Map<NotificationDto>(notification);
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId)
        {
            var notifications = await _notificationRepo.FilterByAsync(new Dictionary<string, object>
            {
                { "UserId", userId }
            });
            // Optional: Order by CreatedAt descending
            notifications = notifications.OrderByDescending(n => n.CreatedAt);

            return notifications.Select(n => _mapper.Map<NotificationDto>(n));
        }

        public async Task<IEnumerable<NotificationDto>> GetUserUnreadNotificationsAsync(string userId)
        {
            var notifications = await _notificationRepo.FilterByAsync(new Dictionary<string, object>
            {
                { "UserId", userId },
                { "IsRead", false }
            });
            notifications = notifications.OrderByDescending(n => n.CreatedAt);

            return notifications.Select(n => _mapper.Map<NotificationDto>(n));
        }

        public async Task<NotificationDto> MarkNotificationAsReadAsync(Guid notificationId, string userId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId);
            if (notification == null)
            {
                throw new KeyNotFoundException($"Notification with ID {notificationId} not found.");
            }

            // Authorization check: Only the owner can mark it as read
            if (notification.UserId != userId)
            {
                await _logService.LogWarningAsync($"Unauthorized attempt to mark notification {notificationId} as read by user {userId}. Owner: {notification.UserId}");
                throw new UnauthorizedAccessException("You are not authorized to mark this notification as read.");
            }

            if (notification.IsRead)
            {
                // Already read, no action needed
                return _mapper.Map<NotificationDto>(notification);
            }

            notification.IsRead = true;
            // notification.UpdatedAt = DateTime.UtcNow; // If you have UpdatedAt on Notification entity
            _notificationRepo.Update(notification);
            await _notificationRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Notification {notificationId} marked as read by user {userId}.");

            return _mapper.Map<NotificationDto>(notification);
        }

        public async Task<IdentityResult> DeleteNotificationAsync(Guid notificationId, string userId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId);
            if (notification == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Notification not found." });
            }

            // Authorization check: Only the owner or an Admin can delete
            // You might add a check for Admin role here using _userManager.IsInRoleAsync(user, "Admin")
            if (notification.UserId != userId)
            {
                await _logService.LogWarningAsync($"Unauthorized attempt to delete notification {notificationId} by user {userId}. Owner: {notification.UserId}");
                return IdentityResult.Failed(new IdentityError { Description = "You are not authorized to delete this notification." });
            }

            _notificationRepo.Delete(notification);
            await _notificationRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Notification {notificationId} deleted by user {userId}.");

            return IdentityResult.Success;
        }
    }
}

