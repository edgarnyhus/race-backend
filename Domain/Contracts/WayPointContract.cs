using System;
using System.Text.Json.Serialization;
using GeoAPI.Geometries;
using Domain.Models;

namespace Domain.Contracts
{
    public class WaypointContract
    {
        public string? id { get; set; }
        public string? alias { get; set; }
        public SignState? state { get; set; } = SignState.Inactive;
        public string? notes { get; set; }
        public LocationContract? location { get; set; }
        [JsonPropertyName("race_id")]
        public string? raceId { get; set; }
        //public Race? race { get; set; }
    }
}