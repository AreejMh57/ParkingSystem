using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ValidateBookingTokenDto
    {
        

        [Required]
        public string UserId { get; set; }
        [Required]
        public string BookingId { get; set; } 

        [Required]
       
        public string Value { get; set; } // قيمة التوكن المقدمة للتحقق
    }
}
