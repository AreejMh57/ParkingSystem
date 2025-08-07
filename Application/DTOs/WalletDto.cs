using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
   public class WalletDto
    {
       public string UserId { get; set; }
        public DateTime CreatedAt
        { get; set; }
        public DateTime UpdatedAt
        { get; set; }
        [Required]
        public Guid WalletId { get; set; }

        [Required]
        [Range(0.0, double.MaxValue, ErrorMessage = "Balance must be non-negative.")]
        public decimal Balance { get; set; }
        public string Email { get; set; }



    }
}
