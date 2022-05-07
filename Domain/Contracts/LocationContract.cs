using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Domain.Contracts
{
    public class LocationContract
    {
        public string? Id { get; set; }
        public string? Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? Timestamp { get; set; }
        [JsonProperty("waypoint_id")]
        public string? WaypointId { get; set; }
        [JsonProperty("sign_id")]
        public string? SignId { get; set; }
    }
}