using System;
using System.Collections.Generic;
using Domain.Interfaces;

namespace Domain.Models;

public class SignType : EntityBase
{
    // public enum SignTypes
    // {
    //     Unknown = 0,
    //     Hazard,             // Annen fare
    //     NoEntry,            // Innkjøring forbudt
    //     InformationBoard,   // Infotavle
    //     NoStops,            // All stans forbudt
    //     NoParking,          // Parkering forbudt
    //     RiskForQueue,       // Fare for kø
    //     Redirect
    // }
   
    public Guid? TenantId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool Reuseable { get; set; }
    public virtual ICollection<Sign>? Signs { get; set; }
}