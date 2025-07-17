using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Notification
    {
        public Guid NotificationId { get; set; }

        [Required]
        public string Channel { get; set; } // "Email", "SMS", etc.

        [Required]
        public string Message { get; set; }

        [Required]
        public bool IsRead { get; set; } = false;       
        [Required]
        public String UserId { get; set; }

        [Required]
        public Guid BookingId { get; set; }


        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public User User { get; set; }

        public Booking Booking { get; set; }



    }
}