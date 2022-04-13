using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Domain.Contracts
{
    public class RaceContract 
    {
        [JsonPropertyName("tenant_id")]
        public string? tenantId { get; set; }
        public string? name { get; set; }
        public string? owner { get; set; }
        [JsonPropertyName("scheduled_at")]
        public DateTime? scheduledAt { get; set; }
        public string? state { get; set; }
        public List<WaypointContract>? waypoints { get; set; }
        public List<SignpostContract>? signposts { get; set; }
        public string? notes { get; set; }
        //public DriverContract? driver { get; set; }
        [JsonPropertyName("organization_id")]
        public string? organizationId { get; set; }
        public Organization? organization { get; set; }
    }
}
