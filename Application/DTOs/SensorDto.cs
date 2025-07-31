using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.DTOs
{
 public class SensorDto
    {
        [Required]
        public Guid SensorId { get; set; }

        public Guid GarageId { get; set; }


        public Sensor.Type SensorType { get; set; } 
        public Sensor.Status AccountStatus { get; set; } 

        public DateTime LastMaintenance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    } 
}
