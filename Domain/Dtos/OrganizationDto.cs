using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Domain.Dtos
{
    public class OrganizationDto : EntityBaseDto
    {
        [JsonProperty("tenant_id")]
        public string? TenantId { get; set; }
        public string? Name { get; set; }
        [JsonProperty("logo_url")]
        public string? LogoUrl { get; set; }
        public string? Identifier { get; set; }
        [JsonProperty("customer_number")]
        public string? CustomerNumber { get; set; }
        [JsonProperty("organization_number")]
        public string? OrganizationNumber { get; set; }
        public int Level { get; set; } = 1;
        [JsonProperty("parent_id")]
        public Guid? ParentId { get; set; }
        public OrganizationDto? Parent { get; set; }
        public virtual ICollection<OrganizationDto>? Children { get; set; }
    }
}
