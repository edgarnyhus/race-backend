﻿using System;
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
        public Guid? SignId { get; set; }
        public Sign? Sign { get; set; }
    }
}