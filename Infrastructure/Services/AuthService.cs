using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Application.DTOs;
using Application.IServices;
using Domain.Entities;
using Domain.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Infrastructure.Authentication;

namespace Infrastructure.Services
{ 
        public class AuthService : IAuthService
        {
            private readonly UserManager<User> _userManager;
            private readonly SignInManager<User> _signInManager;
          
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, JwtTokenGenerator jwtTokenGenerator)
            {
                _userManager = userManager;
                _signInManager = signInManager;
            _jwtTokenGenerator = jwtTokenGenerator;

               
            }

        public async Task<IdentityResult> CreateAccountAsync(RegisterDto dto)
        {
            var user = new User
            {
                
                Email = dto.Email,
                PasswordHash=dto.Password,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            return result;
        }

        public async Task<UserDto> SignInAsync(loginDto dto)
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null)
                return null;

            // login with SignInManager
            var result = await _signInManager.PasswordSignInAsync(
                  dto.Email,
                    dto.Password,
                    dto.RememberMe,
                    lockoutOnFailure: false
                );
                if (!result.Succeeded)
                return null;
            var roles = await _userManager.GetRolesAsync(user);
            string singleRole = roles.FirstOrDefault();
            var token = _jwtTokenGenerator.GenerateToken(user, singleRole);
            return new UserDto
            {
                Token = token,
               
            };
        }
           

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
