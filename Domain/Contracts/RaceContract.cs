using Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Domain.Contracts
{
    public class RaceContract 
    {
        public string? Name { get; set; }
        [JsonProperty("race_day")]
        public int RaceDay { get; set; }
        [JsonProperty("logo_url")]
        public string? LogoUrl { get; set; }
        public string? Notes { get; set; }
        [JsonProperty("created_by")]
        public string? CreatedBy { get; set; }
        [JsonProperty("scheduled_at")]
        public DateTime? ScheduledAt { get; set; }
        public string? State { get; set; }
        //public DriverContract? driver { get; set; }
        [JsonProperty("organization_id")]
        public string? OrganizationId { get; set; }
        [JsonProperty("tenant_id")]
        public string? TenantId { get; set; }
    }
}
