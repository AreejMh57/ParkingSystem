using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.IServices;
using Domain.Entities;
using Domain.IRepositories;
using AutoMapper; // Add this using directive for AutoMapper


namespace Infrastructure.Services
{
    public class GarageService : IGarageService
    {
        private readonly IRepository<Garage> _garageRepo;
        private readonly IMapper _mapper; // Inject IMapper
        private readonly IBookingRepository _bookRepo;
        private readonly ILogService _logService;
        public GarageService(IRepository<Garage> garageRepo, IMapper mapper, ILogService logService, IBookingRepository  bookRepo)
        {
            _garageRepo = garageRepo;
            _mapper = mapper;
            _bookRepo = bookRepo;
            _logService = logService;
           
           

        }

        public async Task<GarageDto> AddGarageAsync(CreateGarageDto createGarageDto)
        {

            var garage = new Garage
            {
                GarageId = Guid.NewGuid(),
                Name = createGarageDto.Name,
                Location = createGarageDto.Location,
                Area = createGarageDto.Area,
                Capacity = createGarageDto.Capacity,
                AvailableSpots = createGarageDto.Capacity,
                PricePerHour = createGarageDto.PricePerHour,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _garageRepo.AddAsync(garage);
            await _garageRepo.SaveChangesAsync();
            // Map the newly created garage entity to GarageDto and return it

            // Return the DTO
            return _mapper.Map<GarageDto>(garage);

        }
        public async Task<GarageDto> UpdateGarageAsync(UpdateGarageDto updateGarageDto)
        {
            var garage = await _garageRepo.GetByIdAsync(updateGarageDto.GarageId);

            if (garage == null)
            {
                // In a service, it's better to throw an exception if the entity isn't found
                // rather than returning null or a default string. The controller will catch it.
                throw new KeyNotFoundException("Garage not found.");
            }

            garage.Name = updateGarageDto.Name;
            garage.Location = updateGarageDto.Location;
            garage.Area = updateGarageDto.Area;
            garage.Capacity = updateGarageDto.Capacity;
            garage.AvailableSpots = updateGarageDto.AvailableSpots;
            garage.PricePerHour = updateGarageDto.PricePerHour;
            garage.UpdatedAt = DateTime.UtcNow;

            //_garageRepo.Update(garage);
            await _garageRepo.SaveChangesAsync();
            // Map the updated garage entity to GarageDto and return it
            return _mapper.Map<GarageDto>(garage); // Return the DTO

        }
        public async Task<IEnumerable<GarageDto>> SearchGaragesAsync(DateTime StartTime, DateTime lastTime,string? city = null, int? minAvailableSpots = null, bool? isActive = null)
        {
            var filters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(city))
            {
                filters.Add("Location", city);
            }
            if (isActive.HasValue)
            {
                filters.Add("IsActive", isActive.Value);
            }

            var garages = await _garageRepo.FilterByAsync(filters);

            if (minAvailableSpots.HasValue)
            {
                garages = garages.Where(g => g.AvailableSpots >= minAvailableSpots.Value);
            }

            return garages.Select(g => _mapper.Map<GarageDto>(g)); // Use mapper here
        }

        public async Task<IEnumerable<GarageDto>> GetAllGaragesAsync()
        {
            var garages = await _garageRepo.GetAllAsync();
            return garages.Select(g => _mapper.Map<GarageDto>(g)); // Use mapper here too
        }





        public async Task ToggleGarageStatusAsync(Guid garageId)
        {
            var garage = await _garageRepo.GetByIdAsync(garageId);
            if (garage == null)
                return;

            garage.IsActive = !garage.IsActive;
            garage.UpdatedAt = DateTime.UtcNow;

            _garageRepo.Update(garage);
            await _garageRepo.SaveChangesAsync();
        }
        public async Task<GarageDto> GetGarageByIdAsync(Guid garageId)
        {
            var garage = await _garageRepo.GetByIdAsync(garageId);
            if (garage == null)
            {
                return null; // Return null if not found, or throw KeyNotFoundException
            }
            return _mapper.Map<GarageDto>(garage); // Use mapper
        }
        public async Task DeleteGarageAsync(Guid garageId)
        {
            var garage = await _garageRepo.GetByIdAsync(garageId);
            if (garage == null)
            {
                throw new KeyNotFoundException("Garage not found.");
            }

            _garageRepo.Delete(garage);
            await _garageRepo.SaveChangesAsync();
        }
        public async Task<IEnumerable<GarageDto>> GetAvailableGaragesAsync(
            DateTime arrivalTime,
            DateTime departureTime,
            double userLat,
            double userLon,
            double? maxDistance)
        {
            // 1. جلب جميع الكراجات النشطة
            var allGarages = await _garageRepo.GetAllAsync();
            var activeGarages = allGarages.Where(g => g.IsActive == true);

            // 2. فلترة الكراجات بناءً على المسافة
            if (maxDistance.HasValue && maxDistance.Value > 0)
            {
                activeGarages = activeGarages.Where(g =>
                    g.Latitude.HasValue && g.Longitude.HasValue &&
                    CalculateDistance(userLat, userLon, g.Latitude.Value, g.Longitude.Value) <= maxDistance.Value
                ).ToList();
            }
            else
            {
                activeGarages = activeGarages.Where(g => g.Latitude.HasValue && g.Longitude.HasValue).ToList();
            }

            var availableGaragesDtos = new List<GarageDto>();

            // 3. التحقق من توفر الأماكن في كل كراج ضمن الفترة الزمنية المحددة
            foreach (var garage in activeGarages)
            {
                if (garage.AvailableSpots <= 0)
                {
                    await _logService.LogInfoAsync($"Garage {garage.Name} ({garage.GarageId}) has no reported available spots.");
                    continue;
                }

                // <--- التصحيح هنا: استخدام _bookingRepo.GetCountAsync() مباشرة مع الشرط --->
                var conflictingBookingsCount = await _bookRepo.GetCountAsync(
                    b => b.GarageId == garage.GarageId &&
                         b.BookingStatus != Booking.Status.Canceled && // <--- تصحيح: استخدام Booking.Status
                         b.StartTime < departureTime &&
                         b.EndTime > arrivalTime
                );

                // عدد الأماكن المتاحة فعلياً خلال الفترة
                var currentAvailableSpots = garage.Capacity - conflictingBookingsCount;

                if (currentAvailableSpots > 0)
                {
                    var garageDto = _mapper.Map<GarageDto>(garage);
                    garageDto.Distance = CalculateDistance(userLat, userLon, garage.Latitude, garage.Longitude);
                    garageDto.AvailableSpots = currentAvailableSpots;
                    availableGaragesDtos.Add(garageDto);
                }
                await _logService.LogInfoAsync($"Garage {garage.Name} ({garage.GarageId}): Calculated {currentAvailableSpots} actual available spots for the period.");
            }

            return availableGaragesDtos.OrderBy(g => g.Distance);
        }

        // <--- دالة مساعدة لحساب المسافة (مثلاً باستخدام Haversine Formula) --->
        // هذا يفترض أن كيان Garage لديه Latitude و Longitude
        private double CalculateDistance(double lat1, double lon1, double? lat2, double? lon2)
        {
            if (!lat2.HasValue || !lon2.HasValue) return double.MaxValue; // إذا لم تكن الإحداثيات موجودة، أعد قيمة كبيرة

            var R = 6371; // نصف قطر الأرض بالكيلومترات
            var dLat = ToRadians(lat2.Value - lat1);
            var dLon = ToRadians(lon2.Value - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2.Value)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c; // المسافة بالكيلومترات
            return distance;
        }

        private double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        /*
        public async Task<IEnumerable<GarageDto>> GetAvailableGaragesAsync(
    DateTime arrivalTime,
    DateTime departureTime,
    double userLat,
    double userLon,
    double? maxDistance)
        {
            // 1. استرجاع الكراجات النشطة ضمن المسافة المحددة
            var garages = await _garageRepo.GetActiveGaragesInRangeAsync(
                userLat,
                userLon,
                maxDistance
            );

            var availableGarages = new List<GarageDto>();

            // 2. التحقق من التوفر لكل كراج
            foreach (var garage in garages)
            {
                // 2.1. حساب عدد الحجوزات المتداخلة
                var bookingCount = await _bookRepo.GetCountAsync(
                    b => b.GarageId == garage.GarageId &&
                         b.Status == BookingStatus.Confirmed &&
                         b.StartTime < departureTime &&
                         b.EndTime > arrivalTime
                );

                // 2.2. التحقق من السعة المتاحة
                if (bookingCount < garage.Capacity)
                {
                    availableGarages.Add(new GarageDto
                    {
                        GarageId = garage.GarageId,
                        Name = garage.Name,
                        Latitude = garage.Latitude,
                        Longitude = garage.Longitude,
                        Capacity = garage.Capacity,
                        PricePerHour = garage.PricePerHour,
                        Distance = CalculateDistance(userLat, userLon, garage.Latitude, garage.Longitude)
                    });
                }
            }

            // 3. ترتيب النتائج حسب المسافة (الأقرب أولاً)
            return availableGarages.OrderBy(g => g.Distance);
        }
        */


    }
}
