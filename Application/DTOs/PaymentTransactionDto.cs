using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.DTOs
{
    public class PaymentTransactionDto
    {
        public Guid TransactionId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public PaymentTransaction.Type Type { get; set; }
        public PaymentTransaction.Status Status { get; set; }

        public string UserId { get; set; }

        public string? TransactionReference { get; set; } 

        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [Required]
        public Guid WalletId { get; set; }

        public Guid BookingId
        {
            get; set;
        }
    }
}