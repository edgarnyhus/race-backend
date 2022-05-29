using Newtonsoft.Json;

namespace Domain.Dtos
{
    public class DriverDto : EntityBaseDto
    {
        [JsonProperty("tenant_id")]
        public string? TenantId { get; set; }
        public OrganizationDto? Organization { get; set; }
        public string? Name { get; set; }
        [JsonProperty("phone_number")]
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public RaceDto? Race { get; set; }
    }
}
