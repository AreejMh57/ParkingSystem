﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Garage
    {
        public Guid GarageId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        [Required]
        [Range(0.0, double.MaxValue)]
        public decimal PricePerHour { get; set; }

        public string Area { get; set; }

        public int AvailableSpots { get; set; }  
        public bool IsActive { get; set; } = true;


        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ICollection<Booking> Bookings { get; set; }

        public ICollection<Sensor> Sensors { get; set; }




    }
}