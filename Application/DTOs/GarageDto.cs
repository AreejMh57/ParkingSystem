using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class GarageDto
    {
        public Guid GarageId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerHour { get; set; }
        public string Area { get; set; }
        public int AvailableSpots { get; set; }
        public bool IsActive { get; set; } 
    }
}
