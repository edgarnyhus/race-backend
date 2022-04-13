using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Domain.Dtos
{
    public class TenantDto : EntityBaseDto
    {
        public string? name { get; set; }
        public string? description { get; set; }
        public string? identifier { get; set; }
        public ICollection<OrganizationDto>? children { get; set; }
    }
}