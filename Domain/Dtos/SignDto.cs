using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Domain.Models;

namespace Domain.Dtos
{
    public class SignDto : EntityBaseDto
    {
        [JsonPropertyName("tenant_id")]
        public string? tenantId { get; set; }
        public string? name { get; set; }
        [JsonPropertyName("sequence_number")]
        public int? SequenceNumber { get; set; }
        [JsonPropertyName("sign_type")]
        public SignType? signType { get; set; }
        public string? qrCode { get; set; }
        public string? notes { get; set; }
        [JsonPropertyName("organization_id")]
        public string? organizationId { get; set; }
        //public Organization? Organization { get; set; }
        [JsonPropertyName("sign_group_id")]
        public string? signGroupId { get; set; }
        //public  SignGroupDto? signGroup { get; set; }
        [JsonPropertyName("signpost_id")]
        public string? signpostId { get; set; }
        //public SignpostDto? signpost { get; set; }
    }
}
