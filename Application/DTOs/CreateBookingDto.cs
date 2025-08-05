using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.DTOs
{
    public class CreateBookingDto
    {
        public string UserId { get; set; }
        public string GarageId { get; set; }
      //  public Guid WalletId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
      //  public Booking.Status BookingStatus { get; set; }
        public string BookingId { get; set; }

    }
}
