using Newtonsoft.Json;
using System.Collections.Generic;

namespace Domain.Contracts
{
    public class TenantContract
    {
        public string? Id { get; set; } = null!;
        public string? Name { get; set; } = null!;
        [JsonProperty("logo_url")]
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }

        /// Comma-separated list of tenant identifiers, i.e. domain, subdomains
        public string? Identifier { get; set; } = null!;

        //public ICollection<OrganizationContract>? organizations { get; set; }
    }
}