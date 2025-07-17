using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.IServices;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogService _logService;
        private readonly IMapper _mapper;

        public UserService(UserManager<User> userManager, ILogService logService, IMapper mapper)
        {
            _userManager = userManager;
            _logService = logService;
            _mapper = mapper;
        }

        public async Task<UserDto> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null; // Or throw KeyNotFoundException
            }
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateUserProfileAsync(string userId, UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await _logService.LogWarningAsync($"Update user profile failed: User {userId} not found.");
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            // Update user properties from DTO
         
            user.Email = dto.Email ?? user.Email;
         
         
            user.UpdatedAt = DateTime.UtcNow; // Update timestamp

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
                await _logService.LogErrorAsync($"Failed to update user {userId} profile: {errors}");
                throw new InvalidOperationException($"Failed to update user profile: {errors}");
            }

            await _logService.LogInfoAsync($"User {userId} profile updated successfully.");
            return _mapper.Map<UserDto>(user);
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            // Optional: Add business rules here (e.g., cannot delete if user has active bookings)
            // Example:
            // var activeBookings = await _bookingRepo.FilterByAsync(new Dictionary<string, object>{{"UserId", userId}, {"Status", Booking.Status.Confirmed}});
            // if (activeBookings.Any()) {
            //     return IdentityResult.Failed(new IdentityError { Description = "Cannot delete user with active bookings." });
            // }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _logService.LogInfoAsync($"User {userId} deleted successfully.");
            }
            else
            {
                var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
                await _logService.LogErrorAsync($"Failed to delete user {userId}: {errors}");
            }
            return result;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList(); // _userManager.Users is IQueryable<User>
            return users.Select(u => _mapper.Map<UserDto>(u));
        }
    }


}
