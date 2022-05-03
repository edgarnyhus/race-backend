using System;
using System.Text.Json.Serialization;

namespace Domain.Contracts
{
    public class DriverContract
    {
        [JsonPropertyName("tenant_id")]
        public string? tenantId { get; set; }
        public string? id { get; set; }
        [JsonPropertyName("organization_id")]
        public string? organizationId { get; set; }
        public OrganizationContract? organization { get; set; }
        public string? name { get; set; }
        [JsonPropertyName("phone_number")]
        public string? phoneNumber { get; set; }
        public string? email { get; set; }
        [JsonPropertyName("race_id")]
        public string? raceId { get; set; }
        public RaceContract? race { get; set; }
    }
}