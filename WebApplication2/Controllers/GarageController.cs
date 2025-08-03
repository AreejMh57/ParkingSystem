using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.IServices;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace YourProjectName.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GarageController : ControllerBase
    {
        private readonly IGarageService _garageService;

        public GarageController(IGarageService garageService)
        {
            _garageService = garageService;
        }

        // #مقابل لـ searchAvailableGarages في Flutter
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableGarages(
            [FromQuery] DateTime arrivalTime,
            [FromQuery] DateTime departureTime,
            [FromQuery] double userLat,
            [FromQuery] double userLon,
            [FromQuery] double? maxDistance = null)
        {
            try
            {
                var garages = await _garageService.GetAvailableGaragesAsync(
                    arrivalTime,
                    departureTime,
                    userLat,
                    userLon,
                    maxDistance
                );
                return Ok(garages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while searching for available garages.");
            }
        }

        // لا يوجد مقابل في Flutter
        [HttpPost("add")]
        public async Task<IActionResult> AddGarage([FromBody] CreateGarageDto createGarageDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var garage = await _garageService.AddGarageAsync(createGarageDto);
                return CreatedAtAction(nameof(GetGarageById), new { garageId = garage.GarageId }, garage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while adding the garage.");
            }
        }

        // لا يوجد مقابل في Flutter
        [HttpGet("all")]
        public async Task<IActionResult> GetAllGarages()
        {
            try
            {
                var garages = await _garageService.GetAllGaragesAsync();
                return Ok(garages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving all garages.");
            }
        }

        // لا يوجد مقابل في Flutter
        [HttpGet("{garageId}")]
        public async Task<IActionResult> GetGarageById(Guid garageId)
        {
            try
            {
                var garage = await _garageService.GetGarageByIdAsync(garageId);
                if (garage == null)
                {
                    return NotFound($"Garage with ID {garageId} not found.");
                }
                return Ok(garage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the garage.");
            }
        }

        // لا يوجد مقابل في Flutter
        [HttpPut("update")]
        public async Task<IActionResult> UpdateGarage([FromBody] UpdateGarageDto updateGarageDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var garage = await _garageService.UpdateGarageAsync(updateGarageDto);
                return Ok(garage);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the garage.");
            }
        }

        // لا يوجد مقابل في Flutter
        [HttpPut("{garageId}/toggle-status")]
        public async Task<IActionResult> ToggleGarageStatus(Guid garageId)
        {
            try
            {
                await _garageService.ToggleGarageStatusAsync(garageId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while toggling garage status.");
            }
        }

        // لا يوجد مقابل في Flutter
        [HttpDelete("{garageId}")]
        public async Task<IActionResult> DeleteGarage(Guid garageId)
        {
            try
            {
                await _garageService.DeleteGarageAsync(garageId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the garage.");
            }
        }
        [HttpGet("search")]
        //[ProducesResponseType(typeof(IEnumerable<GarageDto>), 200)]
     //   [Authorize(Policy = "garage_browse")] // <--- تم إضافة حماية: تتطلب صلاحية 'garage_browse'
        public async Task<IActionResult> SearchGarages(
             [FromQuery] DateTime StartTime,         // من الـQuery String
            [FromQuery] DateTime lastTime,
            [FromQuery] string? city = null, [FromQuery] int? minAvailableSpots = null, [FromQuery] bool? isActive = null)
        {
            try
            {
                var garages = await _garageService.SearchGaragesAsync( StartTime,  lastTime, city, minAvailableSpots, isActive);
                return Ok(garages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while searching for garages.");
            }
        }



        // ... بقية دوال المتحكم (AddGarage, UpdateGarage, SearchGarages, GetAllGarages, ToggleGarageStatus, GetGarageById, DeleteGarage) ...

        /// <summary>
        /// Searches for available garages within a specified time range and distance.
        /// Requires 'GARAGE_BROWSE' permission.
        /// </summary>
        /// <param name="arrivalTime">The desired arrival time for parking.</param>
        /// <param name="departureTime">The desired departure time for parking.</param>
        /// <param name="userLat">User's current latitude.</param>
        /// <param name="userLon">User's current longitude.</param>
        /// <param name="maxDistance">Maximum distance in kilometers (optional).</param>
        /// <returns>A list of available GarageDto.</returns>
        /*
        [HttpGet("available")] // <--- نقطة النهاية الجديدة، المسار الكامل: /api/Garage/available
                               // <--- صلاحية GARAGE_BROWSE تم وضعها على مستوى الكنترولر، يمكن وضعها هنا مباشرة أيضاً --->
                               // [Authorize(Policy = "GARAGE_BROWSE")] 
        public async Task<IActionResult> GetAvailableGarages(
            [FromQuery] DateTime arrivalTime,    // من الـQuery String في الـURL
            [FromQuery] DateTime departureTime,  // من الـQuery String في الـURL
            [FromQuery] double userLat,         // من الـQuery String في الـURL
            [FromQuery] double userLon,         // من الـQuery String في الـURL
            [FromQuery] double? maxDistance = null) // <--- optional (nullable), يأتي من الـQuery String
        {
            // 1. التحقق من صحة المدخلات الأساسية
            if (arrivalTime >= departureTime)
            {
                return BadRequest(new { Message = "Arrival time must be before departure time." });
            }
            // يمكنك إضافة المزيد من التحققات للإحداثيات إذا لزم الأمر (مثلاً userLat بين -90 و 90)
            if (userLat < -90 || userLat > 90 || userLon < -180 || userLon > 180)
            {
                return BadRequest(new { Message = "Invalid user coordinates (latitude/longitude outside valid range)." });
            }
            if (maxDistance.HasValue && maxDistance.Value < 0)
            {
                return BadRequest(new { Message = "Max distance cannot be negative." });
            }

            try
            {
                // 2. استدعاء خدمة GarageService لجلب الكراجات المتاحة
                var garages = await _garageService.GetAvailableGaragesAsync(
                    arrivalTime,
                    departureTime,
                    userLat,
                    userLon,
                    maxDistance
                );
                return Ok(garages); // إرجاع قائمة الكراجات المتاحة كـGarageDto
            }
            catch (Exception ex)
            {
                // 3. معالجة الأخطاء
                // يمكنك هنا استخدام ILogger لتسجيل الخطأ بشكل تفصيلي
                // _logger.LogError(ex, "Error occurred while searching for available garages.");
                return StatusCode(500, new { Message = "An unexpected error occurred while searching for available garages.", Details = ex.Message });
            }
        }
    }

/*
[HttpGet("availability")]
public async Task<IActionResult> CheckAvailability(
[FromQuery] Guid garageId,
[FromQuery] DateTime startTime,
[FromQuery] DateTime endTime)
{
    bool isAvailable = await _garageService.CheckAvailabilityAsync(garageId, startTime, endTime);
    return Ok(new { isAvailable });
}*/

    }
}


//التعديل على ال DTO 
/*
public class GarageDto {
    public Guid GarageId { get; set; }
    public string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Capacity { get; set; } // كان TotalSpots
    public int PricePerHour { get; set; } // إضافة حقل جديد
    // إزالة الحقول غير الضرورية:
    // public int AvailableSpots { get; set; }
    // public bool IsActive { get; set; }
}*/


/*IGarageService:


Task<bool> CheckAvailabilityAsync(Guid garageId, DateTime startTime, DateTime endTime);
*/

/*GarageService:

csharp
public async Task<bool> CheckAvailabilityAsync(Guid garageId, DateTime startTime, DateTime endTime)
{
    // 1. تحقق من وجود الكراج (garageId)
    // 2. تحقق من التداخل الزمني مع الحجوزات الموجودة
    // 3. قارن بعدد الأماكن المتاحة (Capacity)
    // 4. أرجع true/false
}*/

/*public async Task<bool> CheckAvailabilityAsync(Guid garageId, DateTime startTime, DateTime endTime)
{
    // 1. التحقق من وجود الكراج
    var garage = await _garageRepository.GetByIdAsync(garageId);
    if (garage == null) return false;

    // 2. حساب عدد الحجوزات المتداخلة مع الفترة
    var overlappingBookings = await _bookingRepository.GetCountAsync(
        b => b.GarageId == garageId &&
             b.Status == BookingStatus.Confirmed && // الحجوزات المؤكدة فقط
             b.StartTime < endTime &&
             b.EndTime > startTime
    );

    // 3. المقارنة مع السعة المتاحة
    return overlappingBookings < garage.Capacity;
}
*/