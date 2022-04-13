using System.Collections.Generic;
using System.Text.Json.Serialization;
using Domain.Dtos;
using Domain.Models;

namespace Domain.Contracts
{
    public class OrganizationContract
    {
        [JsonPropertyName("tenant_id")]
        public string? tenantId { get; set; }
        public string? id { get; set; }
        public string? name { get; set; }
        public string? Identifier { get; set; }
        [JsonPropertyName("parent_id")]
        public string? parentId { get; set; }
        //public Organization? Parent { get; set; }
        public int level { get; set; } = 1;
        [JsonPropertyName("customer_number")]
        public string? customerNumber { get; set; }
        [JsonPropertyName("organization_number")]
        public string? organizationNumber { get; set; }
        public virtual ICollection<OrganizationContract>? children { get; set; }
    }
}