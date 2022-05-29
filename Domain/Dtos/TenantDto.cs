using System.Collections.Generic;
using Newtonsoft.Json;

namespace Domain.Dtos
{
    public class TenantDto : EntityBaseDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        [JsonProperty("logo_url")]
        public string? LogoUrl { get; set; }
        public string? Identifier { get; set; }
        public ICollection<OrganizationDto>? Children { get; set; }
    }
}