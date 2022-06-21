using System;
using System.Text.Json.Serialization;
using Domain.Models;
using Newtonsoft.Json;

namespace Domain.Contracts
{
    public class SignContract
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        [JsonProperty("sequence_number")]
        public int? SequenceNumber { get; set; } = 1;
        [JsonProperty("qr_code")]
        public string? QrCode { get; set; }
        public SignState? State { get; set; } = SignState.Inactive;
        [JsonProperty("race_day")]
        public int RaceDay { get; set; }
        [JsonProperty("signtype_id")]
        public string SignTypeId { get; set; }
        public LocationContract? Location { get; set; }
        public string? Notes { get; set; }
        public DateTime? LastScanned { get; set; }
        public string? LastScannedBy { get; set; }
        [JsonProperty("race_id")]
        public string? RaceId { get; set; }
        [JsonProperty("signgroup_id")]
        public string? SignGroupId { get; set; }
        [JsonProperty("organization_id")]
        public string? OrganizationId { get; set; }
        [JsonProperty("tenant_id")]
        public string? TenantId { get; set; }
    }
}