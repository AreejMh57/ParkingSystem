using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.DTOs
{
    public class ConfirmedBookingDto
    {
        // بيانات المعاملة (Transaction Data)
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public PaymentTransaction.Type TransactionType { get; set; }
        public PaymentTransaction.Status PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid WalletId { get; set; }
        public string UserId { get; set; }
        public Guid BookingId { get; set; }

        // بيانات التوكن (Token Data)
        public Guid TokenId { get; set; }
        public string TokenValue { get; set; }
        public DateTime TokenValidFrom { get; set; }
        public DateTime TokenValidTo { get; set; }
    }

}
