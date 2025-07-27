using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class User : IdentityUser
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Wallet? Wallet { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<Notification>? Notifications { get; set; }

        public ICollection<Token>? Tokens
        { get; set; }
    }
}