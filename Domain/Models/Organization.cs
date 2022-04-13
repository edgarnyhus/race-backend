#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;
using Domain.Models;

namespace Domain.Models
{
    public class Organization : EntityBase
    {
        [ForeignKey("TenantId")]
        public Guid? TenantId { get; set; }
        public string? Name { get; set; }
        public string? Identifier { get; set; }
        public virtual ICollection<Organization>? Children { get; set; }
        public virtual ICollection<Driver>? Drivers { get; set; }
        public virtual ICollection<Sign>? Signs { get; set; }
        public virtual ICollection<Race>? Routes { get; set; }
        public virtual ICollection<User>? Users { get; set; }
        public Guid? ParentId { get; set; }
        public Organization? Parent { get; set; }
        public int Level { get; set; } = 1;
        public string? CustomerNumber { get; set; }
        public string? OrganizationNumber { get; set; }
    }
}
