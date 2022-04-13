using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Domain.Contracts
{
    public class LocationContract
    {
        public string? id { get; set; }
        public string? address { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public DateTime? timestamp { get; set; }
        [JsonProperty("waypoint_id")]
        public string? waypointId { get; set; }
        [JsonProperty("signpost_id")]
        public string? signpostId { get; set; }
    }
}