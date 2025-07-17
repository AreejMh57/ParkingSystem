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
        public string Channel { get; set; } 
        public string Message { get; set; }

        public bool IsRead { get; set; } 
        public String UserId { get; set; }
    }
}
