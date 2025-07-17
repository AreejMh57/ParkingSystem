using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
   public class CreateSensorDto
    {
        public Guid GarageId { get; set; }

        public enum Type { Entry, Exit, Occupancy } // "Entry", "Exit", "Occupancy"
        public enum Status { Active, Inactive } // "Active", "Inactive"

    }
}
