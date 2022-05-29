using System.Collections.Generic;
using Domain.Interfaces;

namespace Domain.Models
{
    /// Tenant information
    public class Tenant : EntityBase
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }

        /// Comma-separated list of tenant identifiers, i.e. domain, subdomains
        public string Identifier { get; set; } = null!;

        public ICollection<Organization>? Children { get; set; }
    }
}