using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Domain.Dtos;

public class SignTypeDto
{
    //[JsonProperty("tenant_id")]
    //public string? tenantId { get; set; }
    public string? id { get; set; }
    public string? name { get; set; }
    public string? description { get; set; }
    [JsonProperty("image_url")]
    public string? imageUrl { get; set; }
}