// Application/IServices/ITokenService.cs
using Application.DTOs;
using Application.DTOs; // لاستيراد Token DTOs
using Microsoft.AspNetCore.Identity; // لـIdentityResult (لنتائج التحقق)
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface ITokenService
    {
        /// <summary>
        /// Creates and stores a new custom token for a specific user.
        /// </summary>
        /// <param name="dto">DTO containing token creation details (UserId, ExpirationMinutes, BookingId).</param>
        /// <returns>A TokenDto of the newly created token.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the associated User or Booking is not found.</exception>
        Task<TokenDto> CreateCustomTokenAsync(CreateTokenDto dto);

        /// <summary>
        /// Validates a submitted token for a specific user.
        /// </summary>
        /// <param name="dto">DTO containing user ID and the token string.</param>
        /// <returns>An IdentityResult indicating if the token is valid or expired.</returns>
        Task<IdentityResult> ValidateCustomTokenAsync(ValidateBookingTokenDto dto);

        /// <summary>
        /// Retrieves active tokens for a user by BookingId (e.g., for admin review).
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="bookingId">The ID of the booking (optional filter).</param>
        /// <returns>An enumerable collection of active TokenDto.</returns>
        Task<IEnumerable<TokenDto>> GetActiveTokensByUserIdAndBookingIdAsync(string userId, Guid? bookingId = null);

        /// <summary>
        /// Cleans up expired tokens from the database. (e.g., run as a background task).
        /// </summary>
        /// <returns>The number of tokens cleaned up.</returns>
        Task<int> CleanupExpiredTokensAsync();

        // <--- تم إزالة دالة MarkTokenAsUsedAsync لأنها لا تتناسب مع هذا السياق بعد إزالة 'Purpose' --->
        // هي تعتمد على تتبع استخدام لمرة واحدة، لكن without Purpose، يصعب فهم غرض التتبع.
        // وسنضع منطق IsUsed = true داخل ValidateCustomTokenAsync بعد التحقق الناجح
    }
}