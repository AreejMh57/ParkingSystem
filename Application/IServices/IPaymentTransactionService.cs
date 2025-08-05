using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.IServices
{
  public interface IPaymentTransactionService
    {

        Task<ConfirmedBookingDto> ConfirmPaymentAndCreateTokenAsync(CreatePaymentTransactionDto dto);
        /// Records a new payment transaction.

       //   Task<PaymentTransactionDto> RecordPaymentAsync(CreatePaymentTransactionDto dto);

       
        /// Retrieves all payment transactions for a specific user.
      
        Task<IEnumerable<PaymentTransactionDto>> GetUserPaymentHistoryAsync(string userId);

        /// Retrieves a single payment transaction by its ID.
       
        Task<PaymentTransactionDto> GetPaymentByIdAsync(Guid paymentId);
    }
}
