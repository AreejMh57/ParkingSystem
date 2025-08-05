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
        private readonly ILogService _logService;       // Injected Log Service
        private readonly IMapper _mapper;               // Injected AutoMapper
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
            // <--- بداية الـTransaction لضمان الذرية في كل العمليات الأساسية للحجز --->
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                 //   _logger.LogInformation("Attempting to create booking for User {UserId} in Garage {GarageId} (simplified logic).", dto.UserId, dto.GarageId);

                    // 1. التحقق من صحة المدخلات الأساسية
                    if (dto.StartTime >= dto.EndTime)
                    {
                        throw new InvalidOperationException("Booking start time must be before end time.");
                    }

                    // 2. التحقق من وجود المستخدم والكراج
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
                    if (user == null) { throw new KeyNotFoundException($"User with ID {dto.UserId} not found."); }

                    var garage = await _garageRepo.GetByIdAsync(dto.GarageId);
                    if (garage == null) { throw new KeyNotFoundException($"Garage with ID {dto.GarageId} not found."); }

                    // 3. التحقق من توفر المكان في الكراج خلال المدة المذكورة (فقط المتاح تماماً)
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

                    // <--- المنطق المعقد (الدفع، التوكن) تم إزالته هنا --->
                    // لا يوجد خصم للمحفظة، لا يوجد تسجيل لعملية دفع، لا يوجد توليد لتوكن الوصول للحساس

                    // 4. إنشاء كائن الحجز
                    var booking = _mapper.Map<Booking>(dto);
                    booking.BookingId = Guid.NewGuid(); // <--- توليد BookingId جديد من الـBackend
                    booking.TotalPrice = 0.00M; // السعر سيكون صفر لأنه لا يوجد خصم مالي هنا
                    booking.BookingStatus = Booking.Status.Pending; // الحالة الأولية قيد الانتظار
                    booking.CreatedAt = DateTime.UtcNow;
                    booking.UpdatedAt = DateTime.UtcNow;

                    _bookingRepo.AddAsync(booking);
                    await _bookingRepo.SaveChangesAsync(); // حفظ الحجز

                    // 5. تحديث الأماكن المتاحة في الكراج (تقليل عددها)
                    garage.AvailableSpots--;
                    _garageRepo.Update(garage);
                    await _garageRepo.SaveChangesAsync();

                    // <--- تأكيد (Commit) الـTransaction بعد نجاح إنشاء الحجز وتحديث الكراج --->
                    await transaction.CommitAsync();
                  //  _logger.LogInformation("Simplified Booking {BookingId} created successfully for User {UserId} in Garage {GarageId}.", booking.BookingId, dto.UserId, dto.GarageId);

                    return _mapper.Map<BookingDto>(booking); // إرجاع BookingDto
                }
                catch (Exception ex)
                {
                    // في حال حدوث أي خطأ، تراجع عن الـTransaction
                    await transaction.RollbackAsync();
                //    _logger.LogError(ex, "Simplified Booking creation failed for User {UserId} in Garage {GarageId}. Transaction rolled back. Error: {Message}", dto.UserId, dto.GarageId, ex.Message);
                    throw; // أعد رمي الاستثناء ليتم التعامل معه في Controller
                }
            } // نهاية using transaction
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
            // يبدأ هنا block الـ 'using' لـ Transaction.
            // هذا يضمن أن الـ Transaction سيتم التخلص منه (dispose) بشكل صحيح حتى لو حدث خطأ.
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _logger.LogInformation("Attempting to create temporary booking for User {UserId} in Garage {GarageId}.", dto.UserId, dto.GarageId);

                    // 1. التحقق من صحة المدخلات الأساسية
                    // يتحقق من أن وقت بدء الحجز يأتي قبل وقت الانتهاء.
                    if (dto.StartTime >= dto.EndTime)
                    {
                        throw new InvalidOperationException("Booking start time must be before end time.");
                    }

                    // 2. التحقق من وجود المستخدم والكراج
                    // يبحث عن المستخدم في قاعدة البيانات باستخدام الـ UserId ويتضمن المحفظة (Wallet).
                    var user = await _context.Users.Include(u => u.Wallet).FirstOrDefaultAsync(u => u.Id == dto.UserId);
                    // إذا لم يتم العثور على المستخدم، يرمي استثناء.
                    if (user == null)
                    {
                        throw new KeyNotFoundException($"User with ID {dto.UserId} not found.");
                    }

                    // يبحث عن المرآب (Garage) في قاعدة البيانات باستخدام الـ GarageId.
                    var garage = await _garageRepo.GetByIdAsync(Guid.Parse(dto.GarageId));
                    // إذا لم يتم العثور على المرآب، يرمي استثناء.
                    if (garage == null)
                    {
                        throw new KeyNotFoundException($"Garage with ID {dto.GarageId} not found.");
                    }

                    // 3. التحقق من توافر الأماكن
                    if (garage.AvailableSpots <= 0)
                    {
                        throw new InvalidOperationException("No available spots in the garage.");
                    }

                    // 4. إنشاء كائن الحجز
                    // يستخدم AutoMapper لتحويل DTO إلى كائن Booking.
                    var booking = _mapper.Map<Booking>(dto);
                    // يولد معرفًا فريدًا جديدًا للحجز.
                    booking.BookingId = Guid.NewGuid();
                    // يحدد السعر بـ 0.00 لأن هذا حجز مؤقت.
                    booking.TotalPrice = 0.00M;
                    // يحدد حالة الحجز على أنها "قيد الانتظار".
                    booking.BookingStatus = Booking.Status.Pending;
                    // يحدد وقت الإنشاء.
                    booking.CreatedAt = DateTime.UtcNow;
                    // يحدد وقت التحديث.
                    booking.UpdatedAt = DateTime.UtcNow;

                    // يضيف كائن الحجز إلى الـ Repository لتحضيره للحفظ.
                    _bookingRepo.AddAsync(booking);
                    // يحفظ التغييرات في قاعدة البيانات.
                    await _bookingRepo.SaveChangesAsync();

                    // 5. تحديث الأماكن المتاحة في الكراج (تقليل عددها)
                    // يقلل عدد الأماكن المتاحة في المرآب بـ 1.
                    garage.AvailableSpots--;
                    // يحدّث كائن المرآب في الـ Repository.
                    _garageRepo.Update(garage);
                    // يحفظ التغييرات في قاعدة البيانات.
                    await _garageRepo.SaveChangesAsync();

                    // <--- تأكيد (Commit) الـTransaction --->
                    // هنا يتم تأكيد جميع التغييرات التي تمت في الـ Transaction.
                    await transaction.CommitAsync();

                    _logger.LogInformation("Temporary Booking {BookingId} created successfully for User {UserId} in Garage {GarageId}.", booking.BookingId, dto.UserId, dto.GarageId);

                    // يرجع كائن BookingDto الذي تم إنشاؤه.
                    return _mapper.Map<BookingDto>(booking);
                }
                catch (Exception ex)
                {
                    // في حال حدوث أي خطأ، يتم الوصول إلى هذا الـ catch block.
                    // <--- التراجع (Rollback) عن الـTransaction --->
                    // يتراجع عن جميع التغييرات التي تمت داخل الـ Transaction، مما يضمن أن قاعدة البيانات تبقى في حالتها الأصلية.
                    await transaction.RollbackAsync();

                    _logger.LogError(ex, "Temporary Booking creation failed for User {UserId} in Garage {GarageId}. Transaction rolled back. Error: {Message}", dto.UserId, dto.GarageId, ex.Message);
                    // يعيد رمي الاستثناء ليتم التعامل معه في الطبقة الأعلى (مثل الـ Controller).
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

        // ... داخل صنف BookingService

        public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync()
        {
            _logger.LogInformation("Retrieving all bookings for administrative view.");

            // جلب جميع الكيانات (Entities) من الـ Repository
            // يفترض أن التابع GetAllAsync() موجود في IRepository<T>
            var bookings = await _bookingRepo.GetAllAsync();

            // إذا كان الـ DTO يحتاج إلى بيانات من جداول مرتبطة (User, Garage)،
            // ستحتاج إلى طريقة أخرى لجلب البيانات
            // مثلاً:
            // var bookingsWithRelatedData = await _context.Bookings
            //                                       .Include(b => b.User)
            //                                       .Include(b => b.Garage)
            //                                       .ToListAsync();
            // ثم قم بتحويلها باستخدام AutoMapper

            // استخدام AutoMapper لتحويل قائمة الكيانات إلى قائمة من DTOs
            var bookingDtos = _mapper.Map<IEnumerable<BookingDto>>(bookings);

            _logger.LogDebug("Successfully retrieved {Count} bookings.", bookingDtos.Count());

            return bookingDtos;
        }

        // ... بقية التوابع
    }

}