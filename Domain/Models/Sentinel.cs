using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Text;
using Domain.Interfaces;

namespace Domain.Models
{
    public class Sentinel : EntityBase
    {
        [ForeignKey("TenantId")]
        public Guid? TenantId { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public Guid? OrganizationId { get; set; }
        public virtual Organization? Organization { get; set; }
        // public virtual ICollection<Vehicle>? Vehicles { get; set; }
        public Guid? RaceId { get; set; }
        public virtual Race? Race { get; set; }
    }
}