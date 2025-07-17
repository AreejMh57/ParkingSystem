using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UpdatePaymentTransactionDto
    {
        public Guid TransactionId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }


        public enum Type { Deposit, Withdrawal, Payment }


        public enum Status { Pending, Completed, Failed }


        [Required]
        public Guid WalletId { get; set; }

        public Guid BookingId
        {
            get; set;
        }
    }
}