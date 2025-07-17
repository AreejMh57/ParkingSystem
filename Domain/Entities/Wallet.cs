using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Wallet
    {
        public Guid WalletId { get; set; }

        [Required]
        [Range(0.0, double.MaxValue)]
        public decimal Balance { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [Required]
        public String UserId { get; set; }


        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }


        public User User { get; set; }

        public ICollection<PaymentTransaction> Transactions { get; set; }
        







    }
}