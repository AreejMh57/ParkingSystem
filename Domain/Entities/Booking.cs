using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Booking 
    {
        public Guid BookingId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        [Range(0.0, double.MaxValue)]
        public decimal TotalPrice { get; set; }

      
        public enum Status { Pending, Confirmed, Canceled } // "Pending", "Confirmed", canaled, etc.
        [Required]
        public Status BookingStatus { get; set; } = Status.Pending;
        [Required]
        public String UserId { get; set; }

        [Required]
        public Guid GarageId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }



        public ICollection <Notification> Notifications { get; set; }

        public virtual User User { get; set; }

        public virtual Garage Garage { get; set; }

        public Token Token { get; set; }

        public PaymentTransaction PaymentTransaction { get; set; }
       



    }
}