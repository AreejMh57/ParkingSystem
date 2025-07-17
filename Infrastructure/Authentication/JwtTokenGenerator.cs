using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Authentication
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration) {

            _configuration = configuration;

        }
        public string GenerateToken(User user,String Role)
        {   //claims
            var claims = new List<Claim>()
            {
              new Claim(JwtRegisteredClaimNames.Sub, user.Id),
              new Claim(ClaimTypes.NameIdentifier, user.Id),
              new Claim(ClaimTypes.Name, user.UserName ?? ""),
              new Claim(ClaimTypes.Email, user.Email ?? ""),
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

            };
            

                claims.Add(new Claim(ClaimTypes.Role, Role));
            

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));


            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
             issuer: _configuration["Jwt:Issuer"],
              audience: _configuration["Jwt:Audience"],
              claims: claims,
              expires: DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:DurationInDays"])),
               signingCredentials: creds);

            
            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}
    
