using System;
using System.Text.Json.Serialization;
using Domain.Models;
using Newtonsoft.Json;

namespace Domain.Contracts
{
    public class SignContract
    {
        [JsonProperty("tenant_id")]
        public string? TenantId { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        [JsonProperty("sequence_number")]
        public int? SequenceNumber { get; set; } = 1;
        [JsonProperty("signtype_id")]
        public string? SignTypeId { get; set; }
        [JsonProperty("qr_code")]
        public string? QrCode { get; set; }
        public SignState? State { get; set; } = SignState.Inactive;
        public LocationContract? Location { get; set; }
        public string? Notes { get; set; }
        [JsonProperty("race_id")]
        public string? RaceId { get; set; }
        [JsonProperty("signgroup_id")]
        public string? SignGroupId { get; set; }
        [JsonProperty("organization_id")]
        public string? OrganizationId { get; set; }
        public DateTime? LastScanned { get; set; }
        public string? LastScannedBy { get; set; }
    }
}