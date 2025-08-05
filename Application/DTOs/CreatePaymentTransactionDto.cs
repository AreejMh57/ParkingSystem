using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.DTOs
{
    public class CreatePaymentTransactionDto
    {

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }


      
        public string UserId { get; set; }
        public Guid BookingId
        {
            get; set;
        }
    }
}
