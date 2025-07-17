using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
 public class SensorDto
    {
        [Required]
        public Guid SensorId { get; set; }

        public Guid GarageId { get; set; }

        public enum Type { Entry, Exit, Occupancy } // "Entry", "Exit", "Occupancy"
        public enum Status { Active, Inactive } // "Active", "Inactive"

    }
}
