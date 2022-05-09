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
        public string? Name { get; set; }
        public string? Notes { get; set; }
        [JsonProperty("scheduled_at")]
        public DateTime? ScheduledAt { get; set; }
        [JsonProperty("created_by")]
        public string? CreatedBy { get; set; }
        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        public string? State { get; set; }
        //public List<WaypointDto>? Waypoints { get; set; }
        //public List<SignDto>? Signs { get; set; }
        public DriverDto? Driver { get; set; }
        [JsonProperty("organization_id")]
        public string? OrganizationId { get; set; }
        [JsonProperty("tenant_id")]
        public string? TenantId { get; set; }
    }
}
