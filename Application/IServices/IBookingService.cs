using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.IServices;
using Application.DTOs;
using Microsoft.AspNetCore.Identity;



namespace Application.IServices
{
   public interface IBookingService
    {
        // Now returns the created BookingDto
        Task<BookingDto> CreateBookingAsync(CreateBookingDto dto);

        Task<IEnumerable<BookingDto>> GetUserBookingsAsync(string userId);

        
        // Now returns IdentityResult for detailed success/failure info
        Task<IdentityResult> CancelBookingAsync(Guid bookingId, string userId);

        Task<BookingDto> GetBookingByIdAsync(Guid bookingId);
        //   Task<BookingDto?> CheckBookingAvailabilityAsync(BookingDto dto);
        //   Task<BookingDto> CreateBookingBegainAsync(CreateBookingDto dto);
        Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
        Task<string> CancelBookingAsync(Guid bookingId);
    }
}
