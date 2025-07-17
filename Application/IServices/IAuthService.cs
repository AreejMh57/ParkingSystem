using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Application.IServices
{
   public interface IAuthService
    {
        Task<IdentityResult> CreateAccountAsync(RegisterDto dto);
         Task<UserDto> SignInAsync(loginDto dto);
       
        
        Task LogoutAsync();
    }
}
