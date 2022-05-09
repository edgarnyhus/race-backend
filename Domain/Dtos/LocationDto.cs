using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Domain.Dtos
{
    public class LocationDto
    {
        public string? Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        //public DateTime? Timestamp { get; set; }
        [JsonPropertyName("waypoint_id")]
        public string? WaypointId { get; set; }
        [JsonPropertyName("sign_id")]
        public string? SignId { get; set; }
    }
}
