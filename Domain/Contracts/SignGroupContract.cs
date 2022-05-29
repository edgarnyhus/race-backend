using Newtonsoft.Json;

namespace Domain.Contracts;

public class SignGroupContract
{
    [JsonProperty("tenant_id")]
    public string? TenantId { get; set; }
    public string? Name { get; set; }
    //public List<Sign>? Signs { get; set; }
    public string? Notes { get; set; }
    [JsonProperty("organization_id")]
    public string? OrganizationId { get; set; }
    //public  Organization? Organization { get; set; }
}