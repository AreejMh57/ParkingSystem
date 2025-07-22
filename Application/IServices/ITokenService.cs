// Application/IServices/ITokenService.cs
using Application.DTOs;

using Microsoft.AspNetCore.Identity; // لـIdentityResult (لنتائج التحقق)
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface ITokenService
    {
        /// <summary>
        /// Creates and stores a new token for a specific booking.
        /// </summary>
        /// <param name="dto">DTO containing booking ID and expiration minutes.</param>
        /// <returns>A TokenDto of the newly created token.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the associated Booking is not found.</exception>
        Task<TokenDto> CreateBookingTokenAsync(CreateTokenDto dto);

        /// <summary>
        /// Validates a submitted token for a specific booking.
        /// </summary>
        /// <param name="dto">DTO containing booking ID and the token string.</param>
        /// <returns>An IdentityResult indicating if the token is valid or expired.</returns>
        Task<IdentityResult> ValidateBookingTokenAsync(ValidateBookingTokenDto dto);

        /// <summary>
        /// Retrieves active tokens for a booking (e.g., for admin review).
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>An enumerable collection of active TokenDto.</returns>
        Task<IEnumerable<TokenDto>> GetBookingTokensAsync(Guid bookingId);

        /// <summary>
        /// Cleans up expired tokens from the database. (e.g., run as a background task).
        /// </summary>
        /// <returns>The number of tokens cleaned up.</returns>
        Task<int> CleanupExpiredTokensAsync();
    }
}