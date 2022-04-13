using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace Domain.Dtos
{
    public class OrganizationDto : EntityBaseDto
    {
        [JsonPropertyName("tenant_id")]
        public string? tenantId { get; set; }
        public string? name { get; set; }
        public string? identifier { get; set; }
        [JsonPropertyName("parent_id")]
        public Guid? parentId { get; set; }
        //public OrganizationDto? Parent { get; set; }
        public int level { get; set; } = 1;
        [JsonPropertyName("customer_number")]
        public string? customerNumber { get; set; }
        [JsonPropertyName("organization_number")]
        public string? organizationNumber { get; set; }
        public virtual ICollection<OrganizationDto>? children { get; set; }
        public OrganizationDto? parent { get; set; }
        public int Level { get; set; } = 1;
    }
}
