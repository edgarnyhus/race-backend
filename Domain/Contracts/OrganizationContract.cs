using Newtonsoft.Json;

namespace Domain.Contracts
{
    public class OrganizationContract
    {
        [JsonProperty("tenant_id")]
        public string? TenantId { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        [JsonProperty("logo_url")]
        public string? LogoUrl { get; set; }
        public string? Identifier { get; set; }
        [JsonProperty("customer_number")]
        public string? CustomerNumber { get; set; }
        [JsonProperty("organization_number")]
        public string? OrganizationNumber { get; set; }
        public int level { get; set; } = 1;
        [JsonProperty("parent_id")]
        public string? ParentId { get; set; }
        //public virtual ICollection<OrganizationContract>? children { get; set; }
    }
}