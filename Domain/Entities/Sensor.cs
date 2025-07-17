using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Sensor
    {
        [Required]
        public Guid SensorId { get; set; }



        public enum Type { Entry, Exit, Occupancy } // "Entry", "Exit", "Occupancy"


        public enum Status { Active, Inactive } // "Active", "Inactive"

        
        public Status AccountStatus { get; set; } = Status.Active;


        public DateTime? LastMaintenance { get; set; } // Nullable
        public Guid GarageId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Garage Garage { get; set; }




    }
}