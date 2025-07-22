using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class NotificationDto

    {
        public Guid NotificationId { get; set; }
        public string Channel { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } 
        public String UserId { get; set; } = string.Empty;
    }
}
