using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class PaymentTransaction
    {
        public Guid TransactionId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public enum Type { Deposit, Withdrawal, Payment }
        //Financial method type
        public Type TransactionType { get; set; }


        public enum Status { Pending, Completed, Failed }

        // Status of the financial transaction
        public Status PaymentStatus { get; set; }

        // Transaction reference number (e.g., one from an external payment gateway)
        public string? TransactionReference { get; set; }
        ///Date and time of the operation
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;


        [Required]
        public Guid WalletId { get; set; }

        public Guid BookingId { get; set; }
        

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Booking Booking { get; set; }

        public Wallet Wallet { get; set; }

        


    }
}