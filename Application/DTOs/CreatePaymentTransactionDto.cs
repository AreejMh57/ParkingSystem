using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.DTOs
{
    public class CreatePaymentTransactionDto
    {

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }


      
        // Transaction reference number (e.g., one from an external payment gateway)
        public string? TransactionReference { get; set; }
        ///Date and time of the operation
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public PaymentTransaction.Type Type { get; set; } 
        public PaymentTransaction.Status Status { get; set; } 
        [Required]
        public Guid WalletId { get; set; }

        public string UserId { get; set; }
        public Guid BookingId
        {
            get; set;
        }
    }
}
