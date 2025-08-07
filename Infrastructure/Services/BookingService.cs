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
using Microsoft.EntityFrameworkCore;
using Infrastructure.Contexts;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRepository<Booking> _bookingRepo;
        private readonly IRepository<Garage> _garageRepo;
        private readonly IWalletService _walletService; // Injected Wallet Service
        private readonly ILogService _logService;      // Injected Log Service
        private readonly IMapper _mapper;              // Injected AutoMapper
        private readonly AppDbContext _context;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IRepository<Booking> bookingRepo,
            IRepository<Garage> garageRepo,
            IWalletService walletService,
            ILogService logService,
            IMapper mapper,
            AppDbContext context,
            ILogger<BookingService> logger)
        {
            _bookingRepo = bookingRepo;
            _garageRepo = garageRepo;
            _walletService = walletService;
            _logService = logService;
            _mapper = mapper;
            _context = context;
            _logger = logger;
        }

        /*
        public async Task<BookingDto> CreateBookingBegainAsync(CreateBookingDto dto)
        {
            // <--- Start a Transaction to ensure atomicity for all core booking operations --->
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                   // _logger.LogInformation("Attempting to create booking for User {UserId} in Garage {GarageId} (simplified logic).", dto.UserId, dto.GarageId);

                    // 1. Validate basic input
                    if (dto.StartTime >= dto.EndTime)
                    {
                        throw new InvalidOperationException("Booking start time must be before end time.");
                    }

                    // 2. Check for the existence of the user and garage
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
                    if (user == null) { throw new KeyNotFoundException($"User with ID {dto.UserId} not found."); }

                    var garage = await _garageRepo.GetByIdAsync(dto.GarageId);
                    if (garage == null) { throw new KeyNotFoundException($"Garage with ID {dto.GarageId} not found."); }

                    // 3. Check for spot availability in the garage for the specified period (only fully available)
                    var conflictingBookingsCount = await _context.Bookings
                        .Where(b => b.GarageId == dto.GarageId &&
                                    b.BookingStatus != Booking.Status.Canceled &&
                                    (dto.StartTime < b.EndTime && dto.EndTime > b.StartTime))
                        .CountAsync();

                    var currentAvailableSpots = garage.Capacity - conflictingBookingsCount;
                    if (currentAvailableSpots <= 0)
                    {
                        throw new InvalidOperationException("No available spots in the garage for the selected period.");
                    }

                    // <--- The complex logic (payment, token) has been removed here --->
                    // No wallet deduction, no payment transaction logging, no access token generation for the sensor

                    // 4. Create the booking object
                    var booking = _mapper.Map<Booking>(dto);
                    booking.BookingId = Guid.NewGuid(); // <--- Generate a new BookingId from the Backend
                    booking.TotalPrice = 0.00M; // The price will be zero because there's no financial deduction here
                    booking.BookingStatus = Booking.Status.Pending; // Initial status is Pending
                    booking.CreatedAt = DateTime.UtcNow;
                    booking.UpdatedAt = DateTime.UtcNow;

                    _bookingRepo.AddAsync(booking);
                    await _bookingRepo.SaveChangesAsync(); // Save the booking

                    // 5. Update available spots in the garage (reduce the count)
                    garage.AvailableSpots--;
                    _garageRepo.Update(garage);
                    await _garageRepo.SaveChangesAsync();

                    // <--- Commit the Transaction after successful booking creation and garage update --->
                    await transaction.CommitAsync();
                  //  _logger.LogInformation("Simplified Booking {BookingId} created successfully for User {UserId} in Garage {GarageId}.", booking.BookingId, dto.UserId, dto.GarageId);

                    return _mapper.Map<BookingDto>(booking); // Return the BookingDto
                }
                catch (Exception ex)
                {
                    // In case of any error, roll back the Transaction
                    await transaction.RollbackAsync();
                   // _logger.LogError(ex, "Simplified Booking creation failed for User {UserId} in Garage {GarageId}. Transaction rolled back. Error: {Message}", dto.UserId, dto.GarageId, ex.Message);
                    throw; // Rethrow the exception to be handled in the Controller
                }
            } // End of using transaction
        }
        */

        /// <summary>
        /// ///
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        // Returns BookingDto
        /*
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
        */
        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto dto)
        {
            // The 'using' block for the Transaction starts here.
            // This ensures the Transaction is properly disposed of even if an error occurs.
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _logger.LogInformation("Attempting to create temporary booking for User {UserId} in Garage {GarageId}.", dto.UserId, dto.GarageId);

                    // 1. Validate basic input
                    // Checks that the booking start time comes before the end time.
                    if (dto.StartTime >= dto.EndTime)
                    {
                        throw new InvalidOperationException("Booking start time must be before end time.");
                    }

                    // 2. Check for the existence of the user and garage
                    // Searches for the user in the database using the UserId and includes the Wallet.
                    var user = await _context.Users.Include(u => u.Wallet).FirstOrDefaultAsync(u => u.Id == dto.UserId);
                    // If the user is not found, it throws an exception.
                    if (user == null)
                    {
                        throw new KeyNotFoundException($"User with ID {dto.UserId} not found.");
                    }

                    // Searches for the garage in the database using the GarageId.
                    var garage = await _garageRepo.GetByIdAsync(Guid.Parse(dto.GarageId));
                    // If the garage is not found, it throws an exception.
                    if (garage == null)
                    {
                        throw new KeyNotFoundException($"Garage with ID {dto.GarageId} not found.");
                    }

                    // 3. Check for spot availability
                    if (garage.AvailableSpots <= 0)
                    {
                        throw new InvalidOperationException("No available spots in the garage.");
                    }

                    // 4. Create the booking object
                    // Uses AutoMapper to convert the DTO to a Booking object.
                    var booking = _mapper.Map<Booking>(dto);
                    // Generates a new unique ID for the booking.
                    booking.BookingId = Guid.NewGuid();
                    // Sets the price to 0.00 as this is a temporary booking.
                    booking.TotalPrice = 0.00M;
                    // Sets the booking status to "Pending".
                    booking.BookingStatus = Booking.Status.Pending;
                    // Sets the creation time.
                    booking.CreatedAt = DateTime.UtcNow;
                    // Sets the update time.
                    booking.UpdatedAt = DateTime.UtcNow;

                    // Adds the booking object to the Repository, preparing it for saving.
                    _bookingRepo.AddAsync(booking);
                    // Saves the changes to the database.
                    await _bookingRepo.SaveChangesAsync();

                    // 5. Update available spots in the garage (reduce the count)
                    // Reduces the number of available spots in the garage by 1.
                    garage.AvailableSpots--;
                    // Updates the garage object in the Repository.
                    _garageRepo.Update(garage);
                    // Saves the changes to the database.
                    await _garageRepo.SaveChangesAsync();

                    // <--- Commit the Transaction --->
                    // Here, all changes made within the Transaction are confirmed.
                    await transaction.CommitAsync();

                    _logger.LogInformation("Temporary Booking {BookingId} created successfully for User {UserId} in Garage {GarageId}.", booking.BookingId, dto.UserId, dto.GarageId);

                    // Returns the created BookingDto object.
                    return _mapper.Map<BookingDto>(booking);
                }
                catch (Exception ex)
                {
                    // If any error occurs, this catch block is reached.
                    // <--- Rollback the Transaction --->
                    // It rolls back all changes made within the Transaction, ensuring the database remains in its original state.
                    await transaction.RollbackAsync();

                    _logger.LogError(ex, "Temporary Booking creation failed for User {UserId} in Garage {GarageId}. Transaction rolled back. Error: {Message}", dto.UserId, dto.GarageId, ex.Message);
                    // Rethrows the exception to be handled in the upper layer (like the Controller).
                    throw;
                }
            }
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

        // ... inside BookingService class

        public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync()
        {
            _logger.LogInformation("Retrieving all bookings for administrative view.");

            // Fetch all entities from the Repository
            // It's assumed that the GetAllAsync() method exists in IRepository<T>
            var bookings = await _bookingRepo.GetAllAsync();

            // If the DTO needs data from related tables (User, Garage),
            // you'll need another way to fetch the data
            // For example:
            // var bookingsWithRelatedData = await _context.Bookings
            //                                             .Include(b => b.User)
            //                                             .Include(b => b.Garage)
            //                                             .ToListAsync();
            // Then convert them using AutoMapper

            // Use AutoMapper to convert the list of entities into a list of DTOs
            var bookingDtos = _mapper.Map<IEnumerable<BookingDto>>(bookings);

            _logger.LogDebug("Successfully retrieved {Count} bookings.", bookingDtos.Count());

            return bookingDtos;
        }



        public async Task<string> CancelBookingAsync(Guid bookingId)
        {
            try
            {
                _logger.LogInformation("Attempting to cancel booking with ID {BookingId}.", bookingId);

                // 1. Find the booking by its ID
                var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    _logger.LogWarning("Booking with ID {BookingId} not found.", bookingId);
                    return "Booking not found.";
                }

                // 2. Check the booking status
                if (booking.BookingStatus == Booking.Status.Canceled)
                {
                    _logger.LogWarning("Booking with ID {BookingId} is already canceled.", bookingId);
                    return "Booking is already canceled.";
                }

                // 3. Update the booking status to Canceled
                booking.BookingStatus = Booking.Status.Canceled;
                booking.UpdatedAt = DateTime.UtcNow;

                // 4. Save the changes
                _bookingRepo.Update(booking);
                await _bookingRepo.SaveChangesAsync();

                _logger.LogInformation("Booking with ID {BookingId} was successfully canceled.", bookingId);

                // 5. Return a success message
                return "Booking successfully canceled.";
            }
            catch (Exception ex)
            {
                // In case of any error, it is logged and a failure message is returned
                _logger.LogError(ex, "An error occurred while canceling booking {BookingId}.", bookingId);
                return "Failed to cancel booking due to an internal error.";
            }
        }
    }

}