using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.IServices;
using AutoMapper;
using Domain.Entities;
using Domain.IRepositories;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly IRepository<PaymentTransaction> _paymentRepo;
        private readonly ILogService _logService;
        private readonly IMapper _mapper;
        private readonly IRepository<Booking> _bookingRepo; // Needed for booking validation
        private readonly IRepository<Wallet> _walletRepo; // Needed for wallet validation
        private readonly IRepository<User> _userRepo; // Needed for user validation
        private readonly UserManager<User> _userManager;

        public PaymentTransactionService(
            IRepository<PaymentTransaction> paymentRepo,
            ILogService logService,
            IMapper mapper,
            IRepository<Booking> bookingRepo,
            IRepository<Wallet> walletRepo,
            IRepository<User> userRepo,
            UserManager<User> userManager)
        {
            _paymentRepo = paymentRepo;
            _logService = logService;
            _mapper = mapper;
            _bookingRepo = bookingRepo;
            _walletRepo = walletRepo;
            _userRepo = userRepo;
            _userManager = userManager;
        }

        public async Task<PaymentTransactionDto> RecordPaymentAsync(CreatePaymentTransactionDto dto)
        {
            // 1. Validate existence of related entities
            var booking = await _bookingRepo.GetByIdAsync(dto.BookingId);
            if (booking == null)
            {
                await _logService.LogWarningAsync($"Payment recording failed: Booking {dto.BookingId} not found for user {dto.UserId}.");
                throw new KeyNotFoundException($"Related booking with ID {dto.BookingId} not found.");
            }
            var wallet = await _walletRepo.GetByIdAsync(dto.WalletId);
            if (wallet == null)
            {
                await _logService.LogWarningAsync($"Payment recording failed: Wallet {dto.WalletId} not found for user {dto.UserId}.");
                throw new KeyNotFoundException($"Related wallet with ID {dto.WalletId} not found.");
            }
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                await _logService.LogWarningAsync($"Payment recording failed: User {dto.UserId} not found for booking {dto.BookingId}.");
                throw new KeyNotFoundException($"User with ID {dto.UserId} not found.");
            }

                // 2. Validate payment status logic (e.g., cannot record "Completed" with non-positive amount)
                if (dto.Status == PaymentTransaction.Status.Completed && dto.Amount <= 0)
            {
                await _logService.LogWarningAsync($"Payment recording failed: Completed status with non-positive amount {dto.Amount} for booking {dto.BookingId}.");
                throw new InvalidOperationException("Cannot record a 'Completed' payment with zero or negative amount.");
            }

            var payment = new PaymentTransaction
            {
                TransactionId = Guid.NewGuid(),
                WalletId = dto.WalletId,
                BookingId = dto.BookingId,
               
                Amount = dto.Amount,
                TransactionType = dto.Type, // Use Type from DTO (maps to TransactionType in entity)
                PaymentStatus = dto.Status, // Use Status from DTO (maps to PaymentStatus in entity)
                TransactionReference = dto.TransactionReference,
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _paymentRepo.AddAsync(payment);
            await _paymentRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Payment {payment.TransactionId} recorded for booking {dto.BookingId}. Amount: {dto.Amount}, Status: {payment.PaymentStatus}");

            // 3. Update booking status (e.g., from Pending to Confirmed)
            // This logic can be here or in BookingService depending on your design.
            if (booking.BookingStatus == Booking.Status.Pending && payment.PaymentStatus == PaymentTransaction.Status.Completed)
            {
                booking.BookingStatus = Booking.Status.Confirmed;
                booking.UpdatedAt = DateTime.UtcNow;
                _bookingRepo.Update(booking);
                await _bookingRepo.SaveChangesAsync();
                await _logService.LogInfoAsync($"Booking {booking.BookingId} status updated to Confirmed due to payment completion.");
            }

            return _mapper.Map<PaymentTransactionDto>(payment);
        }

        public async Task<IEnumerable<PaymentTransactionDto>> GetUserPaymentHistoryAsync(string userId)
        {
            var payments = await _paymentRepo.FilterByAsync(new Dictionary<string, object> {
                { "UserId", userId }
            });

            return payments.Select(p => _mapper.Map<PaymentTransactionDto>(p));
        }

        public async Task<PaymentTransactionDto> GetPaymentByIdAsync(Guid paymentId)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId);
            if (payment == null)
            {
                return null;
            }
            return _mapper.Map<PaymentTransactionDto>(payment);
        }

        public async Task<PaymentTransactionDto> UpdatePaymentStatusAsync(Guid paymentId, PaymentTransaction.Status newStatus, string? reference = null)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId);
            if (payment == null)
            {
                await _logService.LogWarningAsync($"Payment status update failed: Payment {paymentId} not found.");
                throw new KeyNotFoundException($"Payment with ID {paymentId} not found.");
            }

            // Add business rules for status transitions (e.g., cannot go from Completed to Pending)
            if (payment.PaymentStatus == PaymentTransaction.Status.Completed && newStatus == PaymentTransaction.Status.Pending)
            {
                await _logService.LogWarningAsync($"Invalid status transition for payment {paymentId}: from Completed to Pending.");
                throw new InvalidOperationException("Cannot change status from Completed to Pending.");
            }
            // Example: If a payment failed, it cannot directly become "Completed" without a new process
            // if (payment.PaymentStatus == PaymentTransaction.Status.Failed && newStatus == PaymentTransaction.Status.Completed)
            // {
            //     throw new InvalidOperationException("Payment cannot be 'Completed' directly from 'Failed'. A new transaction might be needed.");
            // }


            payment.PaymentStatus = newStatus;
            payment.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(reference))
            {
                payment.TransactionReference = reference;
            }

            _paymentRepo.Update(payment);
            await _paymentRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Payment {paymentId} status updated to {newStatus}.");

            return _mapper.Map<PaymentTransactionDto>(payment);
        }
    }

}
