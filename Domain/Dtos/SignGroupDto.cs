using System.Collections.Generic;
using Newtonsoft.Json;
using Domain.Models;

namespace Domain.Dtos;

public class SignGroupDto : EntityBaseDto
{
    [JsonProperty("tenant_id")]
    public string? TenantId { get; set; }
    public string? Name { get; set; }
    public List<Sign>? Signs { get; set; }
    public string? Notes { get; set; }
    [JsonProperty("organization_id")]
    public string? OrganizationId { get; set; }
    //public  Organization? Organization { get; set; }
}