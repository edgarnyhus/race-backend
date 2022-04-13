using System.Collections.Generic;
using System.Text.Json.Serialization;
using Domain.Models;

namespace Domain.Dtos;

public class SignGroupDto : EntityBaseDto
{
    [JsonPropertyName("tenant_id")]
    public string? tenantId { get; set; }
    public string? name { get; set; }
    public List<Sign>? signs { get; set; }
    public string? notes { get; set; }
    [JsonPropertyName("organization_id")]
    public string? organizationId { get; set; }
    //public  Organization? organization { get; set; }
}