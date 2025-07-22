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
       

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Booking Booking { get; set; }




    }
}