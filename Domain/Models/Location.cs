using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Text;
using Domain.Interfaces;

namespace Domain.Models
{
    public class Location : EntityBase
    {
        public string? Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? Timestamp { get; set; }
        public Guid? WaypointId { get; set; }
        public Waypoint? Waypoint { get; set; }
        public Guid? SignpostId { get; set; }
        public Signpost? Signpost { get; set; }
    }
}