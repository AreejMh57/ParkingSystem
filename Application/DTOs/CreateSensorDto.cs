using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.DTOs
{
   public class CreateSensorDto
    {
        public Guid GarageId { get; set; }


        public Sensor.Type SensorType { get; set; }
        public Sensor.Status AccountStatus { get; set; }

        public DateTime LastMaintenance { get; set; }

    }
}
