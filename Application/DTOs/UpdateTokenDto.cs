using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UpdateTokenDto
    {

        public Guid TokenId { get; set; }

        [Required]
        public string Value { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        [Required]
        public Guid BookingId { get; set; }
        public String UserId { get; set; }

    }
}
