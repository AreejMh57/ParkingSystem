using Application.DTOs;
using Application.IServices;
using Domain.Entities;
using Domain.IRepositories;
using Microsoft.AspNetCore.Identity; // To include IdentityResult
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IRepository<Garage> _garageRepo;
        private readonly IWalletService _walletService; // Injected Wallet Service
        private readonly ILogService _logService;       // Injected Log Service
        private readonly IMapper _mapper;               // Injected AutoMapper

        public BookingService(
            IRepository<Booking> bookingRepo,
            IRepository<Garage> garageRepo,
            IWalletService walletService,
            ILogService logService,
            IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _garageRepo = garageRepo;
            _walletService = walletService;
            _logService = logService;
            _mapper = mapper;
        }

        // Returns BookingDto
        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto dto)
        {
            // 1. Validate Garage availability
            var garage = await _garageRepo.GetByIdAsync(dto.GarageId);
            if (garage == null || !garage.IsActive || garage.AvailableSpots <= 0)
            {
                await _logService.LogWarningAsync($"Booking failed: Garage {dto.GarageId} is unavailable or full for user {dto.UserId}.");
                // It's usually better to throw an exception in service for invalid input/state.
                throw new InvalidOperationException("Garage is unavailable or full.");
            }

            // 2. Validate time duration and calculate price
            var duration = (dto.EndTime - dto.StartTime).TotalHours;
            if (duration <= 0) throw new InvalidOperationException("Invalid booking duration.");
            var price = (decimal)duration * garage.PricePerHour;
            if (price <= 0) throw new InvalidOperationException("Calculated price is zero or negative.");

            // 3. Deduct funds from the user's wallet
            // The DeductAsync method now throws exceptions on failure, so we rely on that.
            // If no exception is thrown, the deduction was successful.
            var deductionResultDto = await _walletService.DeductAsync(dto.WalletId, price);
            // We can log the successful deduction through its DTO if needed
            await _logService.LogInfoAsync($"Successfully deducted {price} from wallet {dto.WalletId} for user {dto.UserId}. New balance: {deductionResultDto.Balance}");


            // 4. Create the booking entity
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                UserId = dto.UserId,
                GarageId = dto.GarageId,
                // If you have ParkingSpotId in your Booking DTO and entity, include it here:
                // ParkingSpotId = dto.ParkingSpotId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                TotalPrice = price,
                BookingStatus = Booking.Status.Confirmed, // Assuming confirmed after successful payment
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            // 5. Update available spots in the garage
            garage.AvailableSpots -= 1;
            _garageRepo.Update(garage);
            await _garageRepo.SaveChangesAsync();

            await _logService.LogInfoAsync($"New booking {booking.BookingId} created successfully for user {dto.UserId} in garage {dto.GarageId}. Total Price: {price}");

            // Map the created booking entity to BookingDto and return it
            // Ensure navigation properties (User, Garage) are loaded in the repository for proper mapping.
            return _mapper.Map<BookingDto>(booking);
        }

        public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(string userId)
        {
            var result = await _bookingRepo.FilterByAsync(new Dictionary<string, object> {
                { "UserId", userId }
            });

            // Map entities to DTOs using AutoMapper
            // Ensure navigation properties (User, Garage) are loaded in the repository for proper mapping.
            return result.Select(b => _mapper.Map<BookingDto>(b));
        }

        // Returns IdentityResult
        public async Task<IdentityResult> CancelBookingAsync(Guid bookingId, string userId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null) return IdentityResult.Failed(new IdentityError { Description = "Booking not found." });

            // Authorization check
            if (booking.UserId != userId) return IdentityResult.Failed(new IdentityError { Description = "You are not authorized to cancel this booking." });
            if (booking.BookingStatus == Booking.Status.Canceled) return IdentityResult.Failed(new IdentityError { Description = "Booking already canceled." });

            // Business rule: Cannot cancel less than 30 minutes before start
            if ((booking.StartTime - DateTime.UtcNow).TotalMinutes < 30)
            {
                await _logService.LogWarningAsync($"Attempted to cancel booking {bookingId} by user {userId} less than 30 minutes before start.");
                return IdentityResult.Failed(new IdentityError { Description = "Bookings cannot be canceled less than 30 minutes before start time." });
            }

            // Update booking status
            booking.BookingStatus = Booking.Status.Canceled;
            booking.UpdatedAt = DateTime.UtcNow;
            _bookingRepo.Update(booking);
            await _bookingRepo.SaveChangesAsync();

            // Refund logic (if applicable)
            // This is a business decision: Full refund? Partial? No refund?
            // Assuming full refund here, you'd need the WalletId on the Booking entity.
            // And handle exceptions thrown by DepositAsync
            // if (booking.WalletId != Guid.Empty) // Check if WalletId is set on the booking entity
            // {
            //     try
            //     {
            //         // DepositAsync now throws exceptions, so catch them here if refund is optional
            //         await _walletService.DepositAsync(booking.WalletId, booking.TotalPrice);
            //     }
            //     catch (Exception ex)
            //     {
            //         await _logService.LogErrorAsync($"Failed to refund {booking.TotalPrice} for canceled booking {bookingId}. Error: {ex.Message}");
            //         // Decide if refund failure means booking cancellation also fails, or just log.
            //     }
            // }

            // Re-open spot in the garage
            var garage = await _garageRepo.GetByIdAsync(booking.GarageId);
            if (garage != null)
            {
                garage.AvailableSpots += 1;
                _garageRepo.Update(garage);
                await _garageRepo.SaveChangesAsync();
            }

            await _logService.LogInfoAsync($"Booking {booking.BookingId} successfully canceled by user {userId}.");
            return IdentityResult.Success; // Return success
        }

        public async Task<BookingDto> GetBookingByIdAsync(Guid bookingId)
        {
            // For proper mapping of UserName and GarageName, ensure navigation properties are loaded.
            // You might need to adjust your Generic Repository or create a specific one if .Include() is needed here.
            // Example: var booking = await _bookingRepo.GetByIdAsync(bookingId).Include(b => b.User).Include(b => b.Garage);
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return null; // Or throw KeyNotFoundException if you prefer that behavior
            }
            // Map the entity to DTO
            return _mapper.Map<BookingDto>(booking);
        }
    }
}