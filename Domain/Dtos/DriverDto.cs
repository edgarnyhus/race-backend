using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Domain.Dtos
{
    public class DriverDto : EntityBaseDto
    {
        [JsonPropertyName("tenant_id")]
        public string? tenantId { get; set; }
        public OrganizationDto? organization { get; set; }
        public string? name { get; set; }
        [JsonPropertyName("phone_number")]
        public string? phoneNumber { get; set; }
        public string? email { get; set; }
        public RaceDto? race { get; set; }
    }
}
