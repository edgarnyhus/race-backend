using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Domain.Dtos;

public class SignTypeDto
{
    //[JsonProperty("tenant_id")]
    //public string? TenantId { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    [JsonProperty("image_url")]
    public string? ImageUrl { get; set; }
}