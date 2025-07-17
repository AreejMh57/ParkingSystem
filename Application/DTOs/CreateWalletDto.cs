using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
public class CreateWalletDto
    {
        [Required]
        [Range(0.0, double.MaxValue, ErrorMessage = "Balance must be non-negative.")]
        public decimal Balance { get; set; }

        [Required]
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
