using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Application.IServices
{
    public interface INotificationService
    {
        
        /// Creates and records a new notification for a user.
        
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto);

      
        /// Retrieves all notifications for a specific user.
      
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId);

        /// Retrieves only unread notifications for a specific user.
      
        Task<IEnumerable<NotificationDto>> GetUserUnreadNotificationsAsync(string userId);

        
        /// Marks a specific notification as read.
       
        Task<NotificationDto> MarkNotificationAsReadAsync(Guid notificationId, string userId);

        /// <summary>
        /// Deletes a specific notification. (Admin or owner operation)
   
        Task<IdentityResult> DeleteNotificationAsync(Guid notificationId, string userId);
    
}
}
