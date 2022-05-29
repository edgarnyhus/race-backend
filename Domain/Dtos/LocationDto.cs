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
        [JsonProperty("waypoint_id")]
        public string? WaypointId { get; set; }
        [JsonProperty("sign_id")]
        public string? SignId { get; set; }
    }
}
