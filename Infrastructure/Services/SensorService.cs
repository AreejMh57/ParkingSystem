// Infrastructure/Services/SensorService.cs

using Application.IServices;
using Domain.Entities; // Your Sensor entity
using Domain.IRepositories; // IRepository
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Application.DTOs;

namespace Infrastructure.Services
{
    public class SensorService : ISensorService
    {
        private readonly IRepository<Sensor> _sensorRepo;
        private readonly IRepository<Garage> _garageRepo; // To validate GarageId
        private readonly IBookingService _bookingService;
        private readonly ILogService _logService;
        private readonly IMapper _mapper;

        // Inject all dependencies
        public SensorService(
            IRepository<Sensor> sensorRepo,
            IRepository<Garage> garageRepo, // Inject Garage repository
            ILogService logService,
             IBookingService bookingService,
            IMapper mapper)
        {
            _sensorRepo = sensorRepo;
            _garageRepo = garageRepo;
            _bookingService = bookingService;
            _logService = logService;
            _mapper = mapper;
        }

        public async Task<SensorDto> CreateSensorAsync(CreateSensorDto dto)
        {
            // 1. Validate if GarageId exists
            var garage = await _garageRepo.GetByIdAsync(dto.GarageId);
            if (garage == null)
            {
                await _logService.LogWarningAsync($"Sensor creation failed: Garage {dto.GarageId} not found.");
                throw new KeyNotFoundException($"Garage with ID {dto.GarageId} not found.");
            }

            // 2. Create the sensor object
            var sensor = new Sensor
            {
                SensorId = Guid.NewGuid(),
                GarageId = dto.GarageId,
                SensorType = dto.SensorType,
                // Use Type from DTO
                AccountStatus = Sensor.Status.Active, // Set default status for the sensor upon creation
                LastMaintenance = dto.LastMaintenance, // Use LastMaintenance from DTO
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow // Update creation and update timestamps
            };

            await _sensorRepo.AddAsync(sensor);
            await _sensorRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Sensor {sensor.SensorId} created for garage {dto.GarageId}. Type: {dto.SensorType}");

            // 3. Convert the entity to DTO and return it
            return _mapper.Map<SensorDto>(sensor);
        }

        public async Task<IEnumerable<SensorDto>> GetAllSensorsAsync()
        {
            var sensors = await _sensorRepo.GetAllAsync();
            return sensors.Select(s => _mapper.Map<SensorDto>(s));
        }

        public async Task<SensorDto?> GetSensorByIdAsync(Guid sensorId)
        {
            var sensor = await _sensorRepo.GetByIdAsync(sensorId);
            if (sensor == null)
            {
                return null; // Return null if sensor not found
            }
            return _mapper.Map<SensorDto>(sensor);
        }

        public async Task<IEnumerable<SensorDto>> GetSensorsByGarageIdAsync(Guid garageId)
        {
            // 1. Retrieve sensors for the specified garage
            var sensors = await _sensorRepo.FilterByAsync(new Dictionary<string, object>
            {
                { "GarageId", garageId }
            });
            // Optional: Filter only active sensors here if needed
            // sensors = sensors.Where(s => s.AccountStatus == Sensor.Status.Active);

            // 2. Convert entities to DTOs
            return sensors.Select(s => _mapper.Map<SensorDto>(s));
        }

        public async Task<SensorDto> UpdateSensorAsync(Guid sensorId, UpdateSensorDto dto)
        {
            // 1. Retrieve the sensor by its ID
            var sensor = await _sensorRepo.GetByIdAsync(sensorId);
            if (sensor == null)
            {
                await _logService.LogWarningAsync($"Sensor update failed: Sensor {sensorId} not found.");
                throw new KeyNotFoundException($"Sensor with ID {sensorId} not found.");
            }

            // 2. Update sensor properties only if provided in the DTO
            if (dto.SensorType.HasValue)
            {
                sensor.SensorType = dto.SensorType.Value;
            }
            if (dto.AccountStatus.HasValue)
            {
                sensor.AccountStatus = dto.AccountStatus.Value;
            }
            if (dto.LastMaintenance.HasValue)
            {
                sensor.LastMaintenance = dto.LastMaintenance.Value;
            }

            sensor.UpdatedAt = DateTime.UtcNow; // Update timestamp

            // 3. Update the sensor in the database
            _sensorRepo.Update(sensor);
            await _sensorRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Sensor {sensorId} updated. Type: {sensor.SensorType}, Status: {sensor.AccountStatus}");

            // 4. Convert the updated entity to DTO and return it
            return _mapper.Map<SensorDto>(sensor);
        }

        public async Task<bool> DeleteSensorAsync(Guid sensorId)
        {
            // 1. Retrieve the sensor by its ID
            var sensor = await _sensorRepo.GetByIdAsync(sensorId);
            if (sensor == null)
            {
                await _logService.LogWarningAsync($"Sensor deletion failed: Sensor {sensorId} not found.");
                throw new KeyNotFoundException($"Sensor with ID {sensorId} not found.");
            }

            // 2. Delete the sensor from the database
            _sensorRepo.Delete(sensor);
            await _sensorRepo.SaveChangesAsync();
            await _logService.LogInfoAsync($"Sensor {sensorId} deleted successfully.");

            return true; // Return true to indicate success
        }
        public async Task<bool> ReportSensorStatusAsync(SensorStatusReportDto dto)
        {
            var sensor = await _sensorRepo.GetByIdAsync(dto.SensorId);
            if (sensor == null)
            {
                await _logService.LogWarningAsync($"Sensor status report failed: Sensor {dto.SensorId} not found.");
                throw new KeyNotFoundException($"Sensor with ID {dto.SensorId} not found.");
            }

            // 1. مصادقة الحساس (أساسية)
            // افتراض: Sensor entity لديها خاصية SecretKey (string) أو يتم التحقق من مفتاح عام.
            // أو: لديك جدول يربط SensorId بـSecretKey
            // for simplicity, let's assume a predefined key for now, or you need to add SecretKey to Sensor entity.
            // if (sensor.SecretKey != dto.SensorKey) // إذا كان هناك SecretKey في كيان Sensor
            // {
            //     await _logService.LogErrorAsync($"Unauthorized sensor report from {dto.SensorId}: Invalid SensorKey.");
            //     throw new UnauthorizedAccessException("Invalid sensor key.");
            // }

            // 2. تحديث حالة الحساس
            if (sensor.IsOccupied != dto.IsOccupied) // تحديث فقط إذا تغيرت الحالة
            {
                sensor.IsOccupied = dto.IsOccupied;
                sensor.UpdatedAt = dto.EventTimestamp; // استخدام توقيت الحدث من الحساس
                _sensorRepo.Update(sensor);
                await _sensorRepo.SaveChangesAsync();
                await _logService.LogInfoAsync($"Sensor {dto.SensorId} changed status to {(dto.IsOccupied ? "Occupied" : "Free")} at {dto.EventTimestamp}.");
            }
            else
            {
                await _logService.LogInfoAsync($"Sensor {dto.SensorId} reported status unchanged ({dto.IsOccupied}).");
            }

            // 3. تحديث عدد الأماكن المتاحة في الكراج (عبر GarageService)
            var garage = await _garageRepo.GetByIdAsync(sensor.GarageId);
            if (garage != null)
            {
                // إذا تغيرت حالة الحساس، حدث الأماكن المتاحة في الكراج
                if (dto.IsOccupied) // إذا أصبح مشغولاً
                {
                    if (garage.AvailableSpots > 0) garage.AvailableSpots--;
                }
                else // إذا أصبح فارغاً
                {
                    if (garage.AvailableSpots < garage.Capacity) garage.AvailableSpots++;
                }
                _garageRepo.Update(garage);
                await _garageRepo.SaveChangesAsync();
                await _logService.LogInfoAsync($"Garage {garage.GarageId} available spots updated to {garage.AvailableSpots}.");
            }
            else
            {
                await _logService.LogWarningAsync($"Garage {sensor.GarageId} not found for sensor {dto.SensorId}. Cannot update available spots.");
            }

            // 4. تحديث حالة الحجز (عبر BookingService) إذا كان التقرير مرتبطاً بحجز
            if (dto.BookingId.HasValue)
            {
                // هنا يمكن استدعاء دالة في BookingService لتسجيل Check-in/Check-out
                // ملاحظة: دالة CancelBookingAsync موجودة، لكن نحتاج دالة لـCheckIn/CheckOut.
                // سنقوم بإنشاء دالة وهمية الآن، أو يمكن استخدامها لغرض اختبار
                // if (dto.IsOccupied) // إذا كان check-in
                // {
                //     // await _bookingService.CheckInBookingAsync(dto.BookingId.Value, dto.UserId); // تحتاج UserId هنا
                //     // For now, let's assume BookingService handles check-in/out logic
                // } else {
                //     // await _bookingService.CheckOutBookingAsync(dto.BookingId.Value, dto.UserId); // تحتاج UserId هنا
                // }
                await _logService.LogInfoAsync($"Booking {dto.BookingId.Value} check-in/out logic triggered by sensor {dto.SensorId} status change.");
            }

            return true;
        }
    }

}


