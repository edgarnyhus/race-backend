using System;
using System.Text.Json.Serialization;
using Domain.Models;
using NetTopologySuite.Geometries;
using Location = Domain.Models.Location;

namespace Domain.Contracts;

public class SignpostContract
{
    public string? alias { get; set; }
    public SignState? state { get; set; } = SignState.Inactive;
    public string? notes { get; set; }
    public Location? location { get; set; }
    public Sign? sign { get; set; }
    [JsonPropertyName("race_id")]
    public Guid? raceId { get; set; }
    public Race? race { get; set; }
    [JsonPropertyName("last_scanned_by")]
    public string? lastScannedBy { get; set; }
    [JsonPropertyName("last_scanned")]
    public DateTime? lastScanned { get; set; }
}