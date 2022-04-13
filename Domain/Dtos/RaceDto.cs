using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Domain.Models;

namespace Domain.Dtos
{
    public class RaceDto : EntityBaseDto
    {
        [JsonPropertyName("tenant_id")]
        public string? tenantId { get; set; }
        [JsonPropertyName("organization_id")]
        public string? organizationId { get; set; }
        public string? name { get; set; }
        public string? owner { get; set; }
        [JsonPropertyName("scheduled_at")]
        public DateTime? ScheduledAt { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime? createdAt { get; set; }
        [JsonPropertyName("updated_at")]
        public DateTime? updatedAt { get; set; }
        public string? state { get; set; }
        public List<WaypointDto>? waypoints { get; set; }
        public List<SignpostDto>? signposts { get; set; }
        public string? notes { get; set; }
        public DriverDto? driver { get; set; }
    }
}
