﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UpdateUserDto
    {
        [Required]
        public string UserId { get; set; } 

        [Required]
        [EmailAddress]
        public string Email { get; set; }


    }
}

