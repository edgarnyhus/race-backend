using System;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Domain.Interfaces;
using Domain.Models;

namespace Domain.Specifications
{
    public sealed class GetSignsSpecification : BaseSpecification<Sign>
    {
        public GetSignsSpecification(string id) : base(x => x.Id.Equals(id))
        {
            // AddInclude(x => x.Location);
            // AddInclude(x => x.Model);
            // AddInclude(x => x.Temperature);
            // AddInclude(x => x.Organization);
        }

        public GetSignsSpecification(IQueryParameters parameters) : base(parameters)
        {
            if (!string.IsNullOrEmpty(parameters.qr_code))
            {
                var qrCode = parameters.qr_code;
                AddCriteria(c => c.QrCode == qrCode);
            }

            if (parameters.multitenancy)
            {
                if (Guid.TryParse(parameters.tenant_id, out Guid tenantId))
                    AddCriteria(c => c.TenantId == tenantId);
            }

            if (!string.IsNullOrEmpty(parameters.organization_id))
            {
                if (Guid.TryParse(parameters.organization_id, out Guid id))
                    AddCriteria(c => c.OrganizationId == id);
            }

            if (parameters.page_size > 0)
            {
                var page = parameters.page == 0 ? parameters.page : parameters.page - 1;
                ApplyPaging(page * parameters.page_size, parameters.page_size);
            }
        }
    }
}
