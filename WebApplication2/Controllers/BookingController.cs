using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; // For ControllerBase, IActionResult, etc.
using Application.DTOs; // For BookingDto, CreateBookingDto
using Application.IServices; // For IBookingService
using Microsoft.AspNetCore.Authorization; // For [Authorize] attribute
using System; // For Guid
using System.Collections.Generic; // For IEnumerable
using System.Security.Claims; // For ClaimTypes.NameIdentifier
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;


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
        /// <param name="dto">Booking creation details.</param>
        /// <returns>The created BookingDto on success.</returns>
        [HttpPost("create")]
        [Authorize(Policy = "booking_create")] // Requires specific permission to create a booking
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            // Get UserId from the authenticated user's token/claims
            // This ensures the booking is made by the authenticated user.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token. Please log in.");
            }
            dto.UserId = userId; // Assign the authenticated user's ID to the DTO

            try
            {
                var createdBooking = await _bookingService.CreateBookingAsync(dto);
                // HTTP 201 Created is often preferred for resource creation
                return StatusCode(201, createdBooking);
            }
            catch (InvalidOperationException ex)
            {
                // Catches business rule violations (e.g., garage full, invalid duration, payment issues)
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Catches cases where related entities (like Garage, Wallet, User) are not found
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                return StatusCode(500, new { Message = "An unexpected error occurred while creating the booking.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all bookings for the authenticated user.
        /// Requires 'booking_browse' permission.
        /// </summary>
        /// <returns>A list of BookingDto.</returns>
        [HttpGet("my-bookings")]
        [Authorize(Policy = "booking_browse")] // Requires permission to browse bookings
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
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>A BookingDto for the specified booking.</returns>
        [HttpGet("{bookingId}")]
        [Authorize(Policy = "booking_browse")] // Requires permission to browse bookings
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
        /// Requires 'booking_delete' permission (or specific cancel permission).
        /// </summary>
        /// <param name="bookingId">The ID of the booking to cancel.</param>
        /// <returns>Success message or error details.</returns>
        [HttpDelete("cancel/{bookingId}")] // Using DELETE for cancellation
        [Authorize(Policy = "booking_delete")] // Requires permission to delete/cancel bookings
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
                // If IdentityResult indicates failure, extract errors
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Message = "Failed to cancel booking.", Errors = errors });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex) // From service if user is not authorized
            {
                return Forbid(ex.Message); // HTTP 403 Forbidden
            }
            catch (InvalidOperationException ex) // From service if business rule violated (e.g., too close to start)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred while canceling the booking.", Details = ex.Message });
            }
        }
    }
}

