﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Domain.Models;
using Newtonsoft.Json;

namespace Domain.Dtos
{
    public class SignDto : EntityBaseDto
    {
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
        public SignTypeDto? SignType { get; set; }
        public LocationDto? Location { get; set; }
        public string? Notes { get; set; }
        [JsonProperty("last_scanned")]
        public DateTime? LastScanned { get; set; }
        [JsonProperty("last_scanned_by")]
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
