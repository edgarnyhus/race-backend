using Newtonsoft.Json;

namespace Domain.Contracts
{
    public class DriverContract
    {
        [JsonProperty("tenant_id")]
        public string? TenantId { get; set; }
        public string? Id { get; set; }
        [JsonProperty("organization_id")]
        public string? OrganizationId { get; set; }
        public OrganizationContract? Organization { get; set; }
        public string? Name { get; set; }
        [JsonProperty("phone_number")]
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        [JsonProperty("race_id")]
        public string? RaceId { get; set; }
        public RaceContract? Race { get; set; }
    }
}