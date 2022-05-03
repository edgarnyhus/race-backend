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
        [JsonProperty("tenant_id")]
        public string? tenantId { get; set; }
        [JsonProperty("organization_id")]
        public string? organizationId { get; set; }
        public string? name { get; set; }
        [JsonProperty("created_by")]
        public string? createdBy { get; set; }
        [JsonProperty("scheduled_at")]
        public DateTime? scheduledAt { get; set; }
        [JsonProperty("created_at")]
        public DateTime? createdAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTime? updatedAt { get; set; }
        public string? state { get; set; }
        //public List<WaypointDto>? waypoints { get; set; }
        //public List<SignDto>? signs { get; set; }
        public string? notes { get; set; }
        public DriverDto? driver { get; set; }
    }
}
