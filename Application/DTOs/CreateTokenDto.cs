using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreateTokenDto
    {
        [Required]
        public String UserId { get; set; }  

        [Required]
        public Guid BookingId { get; set; }
        [Required]
        public int ExpirationMinutes { get; set; }

    }
}
