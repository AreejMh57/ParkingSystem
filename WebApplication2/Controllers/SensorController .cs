// Presentation/Controllers/SensorController.cs
using Application.DTOs;

using Application.IServices; // لـISensorService
using Microsoft.AspNetCore.Authorization; // لـ[Authorize] attribute
using Microsoft.AspNetCore.Mvc; // لـControllerBase, IActionResult, إلخ
using System; // لـGuid
using System.Collections.Generic; // لـIEnumerable
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [ApiController] // يشير إلى أن هذا المتحكم يستجيب لطلبات API الويب
    [Route("api/[controller]")] // يحدد المسار الأساسي لهذا المتحكم (مثلاً: /api/Sensor)
    [Authorize(Roles = "Admin,ParkingManager")] // حماية عامة: فقط Admin أو ParkingManager يمكنهم الوصول
    public class SensorController : ControllerBase
    {
        private readonly ISensorService _sensorService;

        public SensorController(ISensorService sensorService)
        {
            _sensorService = sensorService;
        }

        /// <summary>
        /// Creates a new sensor record.
        /// Requires 'sensor_create' permission.
        /// </summary>
        /// <param name="dto">Sensor creation details.</param>
        /// <returns>The created SensorDto on success.</returns>
        [HttpPost("create")]
        [Authorize(Policy = "sensor_create")] // تتطلب صلاحية إنشاء حساسات
        public async Task<IActionResult> CreateSensor([FromBody] CreateSensorDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var newSensor = await _sensorService.CreateSensorAsync(dto);
                return StatusCode(201, newSensor); // HTTP 201 Created
            }
            catch (KeyNotFoundException ex) // إذا كان الكراج (GarageId) غير موجود
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the sensor.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all sensor records in the system.
        /// Requires 'sensor_browse' permission.
        /// </summary>
        /// <returns>A list of SensorDto.</returns>
        [HttpGet("all")]
        [Authorize(Policy = "sensor_browse")] // تتطلب صلاحية تصفح الحساسات
        public async Task<IActionResult> GetAllSensors()
        {
            try
            {
                var sensors = await _sensorService.GetAllSensorsAsync();
                return Ok(sensors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving all sensors.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a single sensor record by its ID.
        /// Requires 'sensor_browse' permission.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor.</param>
        /// <returns>A SensorDto, or null if not found.</returns>
        [HttpGet("{sensorId}")]
        [Authorize(Policy = "sensor_browse")] // تتطلب صلاحية تصفح الحساسات
        public async Task<IActionResult> GetSensorById(Guid sensorId)
        {
            try
            {
                var sensor = await _sensorService.GetSensorByIdAsync(sensorId);
                if (sensor == null)
                {
                    return NotFound(new { Message = $"Sensor with ID {sensorId} not found." });
                }
                return Ok(sensor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the sensor.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all sensors for a specific garage.
        /// Requires 'sensor_browse' permission.
        /// </summary>
        /// <param name="garageId">The ID of the garage.</param>
        /// <returns>A list of SensorDto.</returns>
        [HttpGet("by-garage/{garageId}")]
        [Authorize(Policy = "sensor_browse")] // تتطلب صلاحية تصفح الحساسات
        public async Task<IActionResult> GetSensorsByGarageId(Guid garageId)
        {
            try
            {
                var sensors = await _sensorService.GetSensorsByGarageIdAsync(garageId);
                return Ok(sensors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving sensors by garage ID.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Updates the status or other details of an existing sensor.
        /// Requires 'sensor_update' permission.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to update.</param>
        /// <param name="dto">DTO containing updated sensor status.</param>
        /// <returns>The updated SensorDto.</returns>
        [HttpPut("{sensorId}/update")]
        [Authorize(Policy = "sensor_update")] // تتطلب صلاحية تعديل الحساسات
        public async Task<IActionResult> UpdateSensor(Guid sensorId, [FromBody] UpdateSensorDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var updatedSensor = await _sensorService.UpdateSensorAsync(sensorId, dto);
                return Ok(updatedSensor);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the sensor.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a sensor record from the system.
        /// Requires 'sensor_delete' permission.
        /// </summary>
        /// <param name="sensorId">The ID of the sensor to delete.</param>
        /// <returns>Success message.</returns>
        [HttpDelete("{sensorId}")]
        [Authorize(Policy = "sensor_delete")] // تتطلب صلاحية حذف الحساسات
        public async Task<IActionResult> DeleteSensor(Guid sensorId)
        {
            try
            {
                var result = await _sensorService.DeleteSensorAsync(sensorId);
                if (result)
                {
                    return NoContent(); // HTTP 204 No Content for successful deletion
                }
                return BadRequest(new { Message = "Failed to delete sensor." }); // إذا عادت الخدمة بـfalse
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the sensor.", Details = ex.Message });
            }
        }


        /// <summary>
        /// Receives status reports from a sensor (e.g., occupied/free).
        /// This endpoint is called by the physical sensor or a gateway.
        /// (Protected by SensorKey in DTO for now, could be API Key Auth in production)
        /// </summary>
        /// <param name="dto">Sensor status report details.</param>
        /// <returns>200 OK if report processed, or error.</returns>
        [HttpPost("report-status")]
        [AllowAnonymous] // <--- للسماح للحساسات غير المصادقة بالوصول (يجب تغييرها في الإنتاج)
        // في الإنتاج: [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ReportSensorStatus([FromBody] SensorStatusReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var success = await _sensorService.ReportSensorStatusAsync(dto);
                if (success)
                {
                    return Ok(new { Message = "Sensor status reported successfully." });
                }
                return BadRequest(new { Message = "Failed to process sensor status report." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex) // إذا كان مفتاح الحساس غير صحيح
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred while reporting sensor status.", Details = ex.Message });
            }
        }
    }
}   