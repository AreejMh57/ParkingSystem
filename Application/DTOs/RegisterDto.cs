using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
   public class RegisterDto
    {
        
        [Required]
        [EmailAddress(ErrorMessage = "Email Address not valid.")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "the passwords is not matched.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }


        [Required]
        public string Role { get; set; } = "Customer"; // "Admin", "ParkingManager", "Customer"


    }
}
