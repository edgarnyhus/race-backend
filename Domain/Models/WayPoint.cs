using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Domain.Interfaces;
using NetTopologySuite.Geometries;


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
