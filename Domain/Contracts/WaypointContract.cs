using Newtonsoft.Json;

namespace Domain.Contracts
{
    public class WaypointContract
    {
        public string? Id { get; set; }
        public string? Alias { get; set; }
        public string? Notes { get; set; }
        public LocationContract Location { get; set; }
        [JsonProperty("race_id")]
        public string? RaceId { get; set; }
        //public Race? Race { get; set; }
    }
}