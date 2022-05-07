using System;
using System.Text.Json.Serialization;
using Domain.Models;

namespace Domain.Dtos
{
    public class WaypointDto : EntityBaseDto
    {
        public string? Alias { get; set; }
        public string? Notes { get; set; }
        public LocationDto? Location { get; set; }
        [JsonPropertyName("race_id")]
        public string? RaceId { get; set; }
    }
}
