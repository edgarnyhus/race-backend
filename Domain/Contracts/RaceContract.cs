using Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Domain.Contracts
{
    public class RaceContract 
    {
        [JsonProperty("tenant_id")]
        public string? tenantId { get; set; }
        public string? name { get; set; }
        [JsonProperty("created_by")]
        public string? createdBy { get; set; }
        [JsonProperty("scheduled_at")]
        public DateTime? scheduledAt { get; set; }
        public string? state { get; set; }
        public string? notes { get; set; }
        //public DriverContract? driver { get; set; }
        [JsonPropertyName("organization_id")]
        public string? organizationId { get; set; }
    }
}
