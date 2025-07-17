using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
   public class UpdateBookingDto
    {
        public DateTime NewStartTime { get; set; }
        public DateTime NewEndTime { get; set; }
    }
}
