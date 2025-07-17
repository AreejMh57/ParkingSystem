using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Authentication
{
    public interface IJwtTokenGenerator
    {
        public string GenerateToken(User user, String Role);
    }
}
