﻿#nullable enable
using System;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;
using Domain.Interfaces;

namespace Domain.Models
{
    public enum SignState
    {
        Unknown = 0,
        Inactive,
        Active,
        Discarded
    }

    public class Sign : EntityBase
    {
        [ForeignKey("TenantId")]
        public Guid? TenantId { get; set; }
        public string? Name { get; set; }
        public int? SequenceNumber { get; set; } = 1;
        public int RaceDay { get; set; }
        public Guid? SignTypeId { get; set; }
        public SignType? SignType { get; set; }
        public string? QrCode { get; set; }
        public SignState? State { get; set; } = SignState.Inactive;
        public Location? Location { get; set; }
        public Point? GeoLocation { get; set; }
        public string? Notes { get; set; }
        public Guid? RaceId { get; set; }
        public Race? Race { get; set; }
        //public ICollection<Race>? Races { get; set; }
        public Guid? SignGroupId { get; set; }
        public SignGroup? SignGroup { get; set; }
        public Guid? OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public DateTime? LastScanned { get; set; }
        public string? LastScannedBy { get; set; }
    }
}