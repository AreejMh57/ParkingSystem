using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Infrastructure.Services;

namespace Peresentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingService> _logger;

        public BookingController(IBookingService bookingService, ILogger<BookingService> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }
        /*
        [HttpPost("create")]
        public async Task<ActionResult<BookingDto>> CreateBooking([FromBody] CreateBookingDto dto)
        {
            try
            {
                // استدعاء خدمة الحجز لإنشاء حجز مؤقت
                var bookingDto = await _bookingService.CreateBookingAsync(dto);

                // عند النجاح، يعود برمز حالة 201 Created مع الكائن الذي تم إنشاؤه.
                // هذا هو الرد القياسي لإنشاء مورد جديد، ويعود بكائن الـ DTO.
                return CreatedAtAction(nameof(GetBookingById), new { id = bookingDto.BookingId }, bookingDto);
            }
            catch (InvalidOperationException ex)
            {
                // في حالة وجود بيانات غير صالحة، يعود برمز حالة 400 Bad Request.
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                // في حالة عدم العثور على المستخدم أو الكراج، يعود برمز حالة 404 Not Found.
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // في حالة وجود أي خطأ غير متوقع، يعود برمز حالة 500 Internal Server Error.
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }*/

        /// <summary>
        /// يسترجع تفاصيل حجز معين باستخدام معرفه.
        /// </summary>
        /// <remarks>
        /// تم تعديل اسم المعلمة من `bookingId` إلى `id` ليتطابق مع التابع `CreatedAtAction`.
        /// </remarks>
        /// <param name="id">معرف الحجز.</param>
        /// <returns>ActionResult مع كائن الحجز أو NotFound إذا لم يتم العثور عليه.</returns>
        /*[HttpGet("{id}")]
        public async Task<ActionResult<BookingDto>> GetBookingById(Guid id)
        {
            // هذه مجرد بنية هيكلية (scaffold)
            // ستحتاج إلى تطبيق منطق استرجاع الحجز من الخدمة.
            var booking = await _bookingService.GetBookingByIdAsync(id);
             if (booking == null)
             {
                return NotFound();
             }
             return Ok(booking);

            // إرجاع NotFound() بشكل مؤقت حتى يتم تنفيذ المنطق.
            return NotFound();
        }*/

        /*
        [HttpPost]
        [ProducesResponseType(typeof(BookingDto), 201)] // Created
        [ProducesResponseType(400)] // Bad Request
        [ProducesResponseType(404)] // Not Found
        [ProducesResponseType(500)] // Internal Server Error
        public async Task<ActionResult<BookingDto>> CreateBooking([FromBody] BookingDto dto)
        {
            _logger.LogInformation("Received request to create a booking for user {UserId} at garage {GarageId}.", dto.UserId, dto.GarageId);

            try
            {
                // Call the booking creation service.
                var booking = await _bookingService.CreateBookingAsync(dto);

                _logger.LogInformation("Successfully created booking {BookingId}.", booking.BookingId);

                // Return an HTTP 200 OK response with the booking object.
                return Ok(booking);
            }
            catch (InvalidOperationException ex)
            {
                // This error occurs if times are invalid or no spots are available.
                _logger.LogWarning(ex, "Invalid operation during booking creation: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // This error occurs if the user or garage is not found.
                _logger.LogWarning(ex, "Resource not found during booking creation: {Message}", ex.Message);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // Any other unexpected errors.
                _logger.LogError(ex, "An unexpected error occurred while creating a booking: {Message}", ex.Message);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        // -----
        */


        /// <summary>
        /// Creates a new parking booking.
        /// Requires 'booking_create' permission.
        /// </summary>

        [HttpPost("create")]
        // [Authorize(Policy = "booking_create")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token. Please log in.");
            }
            dto.UserId = userId;

            try
            {
                var createdBooking = await _bookingService.CreateBookingAsync(dto);
                return StatusCode(201, createdBooking);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred", Details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all bookings for the authenticated user.
        /// Requires 'booking_browse' permission.
        /// </summary>
        [HttpGet("my-bookings")]
        // [Authorize(Policy = "BOOKING_BROWSE")]
        [Authorize]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token. Please log in.");
            }

            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(bookings);
        }

        /// <summary>
        /// Retrieves a specific booking by ID.
        /// Requires 'booking_browse' permission.
        /// </summary>
        [HttpGet("{bookingId}")]
        //  [Authorize(Policy = "booking_browse")]
        public async Task<IActionResult> GetBookingById(Guid bookingId)
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound(new { Message = $"Booking with ID {bookingId} not found." });
            }
            return Ok(booking);
        }


      









            /*

        /// <summary>
        /// Cancels a specific booking.
        /// Requires 'booking_delete' permission.
        /// </summary>
        [HttpDelete("cancel/{bookingId}")]
        //   [Authorize(Policy = "booking_delete")]
        public async Task<IActionResult> CancelBooking(Guid bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token. Please log in.");
            }

            try
            {
                var result = await _bookingService.CancelBookingAsync(bookingId, userId);
                if (result.Succeeded)
                {
                    return Ok(new { Message = "Booking canceled successfully." });
                }
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Message = "Failed to cancel booking", Errors = errors });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred", Details = ex.Message });
            }
        }
    }*/
        }
    }
    