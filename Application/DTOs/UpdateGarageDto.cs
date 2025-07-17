using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UpdateGarageDto
    {
        public Guid GarageId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Location { get; set; }
       [Range(1, int.MaxValue)]
     public int Capacity { get; set; }

        public int AvailableSpots { get; set; }
        [Range(1, int.MaxValue)]

        public decimal PricePerHour { get; set; }
        public string Area { get; set; }
    }
}
