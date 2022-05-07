using System;
using System.Text.Json.Serialization;
using GeoAPI.Geometries;
using Domain.Models;

namespace Domain.Contracts
{
    public class WaypointContract
    {
        public string? Id { get; set; }
        public string? Alias { get; set; }
        public string? Notes { get; set; }
        public LocationContract Location { get; set; }
        [JsonPropertyName("race_id")]
        public string? RaceId { get; set; }
        //public Race? Race { get; set; }
    }
}