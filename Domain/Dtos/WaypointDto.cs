using System;
using System.Text.Json.Serialization;
using Domain.Models;

namespace Domain.Dtos
{
    public class WaypointDto : EntityBaseDto
    {
        public string? alias { get; set; }
        public string? notes { get; set; }
        public LocationDto? location { get; set; }
        [JsonPropertyName("race_id")]
        public string? raceId { get; set; }
        //public Race? race { get; set; }
    }
}
