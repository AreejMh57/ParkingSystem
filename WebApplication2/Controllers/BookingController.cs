using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Peresentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

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
        //  [Authorize(Policy = "booking_browse")]
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
    }
}