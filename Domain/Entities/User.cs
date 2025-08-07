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

      // public DateTime? BanEndDate { get; set; } // Temporary ban end date
       // public bool IsPermanentlyBanned { get; set; } //Is it permanently banned?
       // public int TemporaryBanCount { get; set; } // Number of times temporarily banned
    }
}