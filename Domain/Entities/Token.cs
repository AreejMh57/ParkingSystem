using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Token
    {
        public Guid TokenId { get; set; }

        [Required]
        public string Value { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        [Required]
        public Guid BookingId { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; 
        public Booking Booking { get; set; }


        [Required]
        public string UserId { get; set; }

        public bool IsUsed { get; set; } = false;
        public  User User { get; set; }



    }
}