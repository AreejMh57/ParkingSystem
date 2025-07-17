using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Application.IServices
{
    public interface IUserService
    {
        // Retrieves a user's profile details.
        
        Task<UserDto> GetUserProfileAsync(string userId);

       
        // Updates a user's profile information.
                Task<UserDto> UpdateUserProfileAsync(string userId, UpdateUserDto dto);

       
        // Deletes a user from the system. (Usually an Admin-level operation)
     
        Task<IdentityResult> DeleteUserAsync(string userId);

        // Retrieves a list of all users. (Admin-level operation)
 
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
    }

}
