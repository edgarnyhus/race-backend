using System.Text.Json.Serialization;

namespace Domain.Dtos;

public class SignTypeDto
{
    [JsonPropertyName("tenant_id")]
    public string? tenantId { get; set; }
    public string? id { get; set; }
    public string? name { get; set; }
    public string? description { get; set; }
    [JsonPropertyName("image_url")]
    public string? imageUrl { get; set; }
}