using System;
using Domain.Interfaces;
using NetTopologySuite.Geometries;

namespace Domain.Models;


public class Signpost : EntityBase
{
    public string? Alias { get; set; }
    public SignState? State { get; set; } = SignState.Inactive;
    public string? Notes { get; set; }
    public Location? Location { get; set; }
    public Point? GeoLocation { get; set; }
    public Sign? Sign { get; set; }
    public Guid? RaceId { get; set; }
    public Race? Race { get; set; }
    public string? LastScannedBy { get; set; }
    public DateTime? LastScanned { get; set; }
}