using Newtonsoft.Json;

namespace Domain.Dtos
{
    public class WaypointDto : EntityBaseDto
    {
        public string? Alias { get; set; }
        public string? Notes { get; set; }
        public LocationDto? Location { get; set; }
        [JsonProperty("race_id")]
        public string? RaceId { get; set; }
    }
}
