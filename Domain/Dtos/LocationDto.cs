using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Domain.Dtos
{
    public class LocationDto
    {
        public string? address { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public DateTime? timestamp { get; set; }
        [JsonPropertyName("waypoint_id")]
        public string? waypointId { get; set; }
        [JsonPropertyName("signpost_id")]
        public string? signpostId { get; set; }
    }
}
