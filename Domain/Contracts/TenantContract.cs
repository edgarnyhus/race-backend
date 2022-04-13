using Newtonsoft.Json;
using System.Collections.Generic;

namespace Domain.Contracts
{
    public class TenantContract
    {
        public string? id { get; set; } = null!;
        public string? name { get; set; } = null!;
        public string? description { get; set; }

        /// Comma-separated list of tenant identifiers, i.e. domain, subdomains
        public string? identifier { get; set; } = null!;

        public ICollection<OrganizationContract>? organizations { get; set; }
    }
}