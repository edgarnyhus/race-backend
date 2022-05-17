using System;
using Domain.Interfaces;


namespace Domain.Models
{
    public class Waypoint : EntityBase
    {
        public string? Alias { get; set; }
        public string? Notes { get; set; }
        public Location? Location { get; set; }
        public Guid? RaceId { get; set; }
        public Race? Race { get; set; }
    }
}
