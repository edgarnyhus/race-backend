using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Helpers
{
    public class TenantInfo
    {
        public Guid? TenantId { get; set; }
        public Guid? OrganizationId { get; set; }
    }
}
