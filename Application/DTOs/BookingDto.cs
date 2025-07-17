using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.DTOs
{
      public class BookingDto
    {
        public Guid BookingId { get; set; }
        public Guid GarageId { get; set; }
        public string UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
      
        public Booking.Status BookingStatus { get; set; }

    }
}
