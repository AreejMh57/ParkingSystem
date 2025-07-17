using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreateNotificationDto
    {


        public String UserId { get; set; }

        public Guid BookingId { get; set; }

        [Required]
        public string Channel { get; set; }
        [Required]
        public string Message { get; set; }

    }
}
