﻿using System;
using Domain.Interfaces;
using Domain.Models;

namespace Domain.Specifications
{
    public sealed class GetSentinelsSpecification : BaseSpecification<Sentinel>
    {
        public GetSentinelsSpecification(string id) : base(x => x.Id.Equals(id))
        {

        }

        public GetSentinelsSpecification(IQueryParameters parameters) : base(parameters)
        {
            if (parameters.multitenancy)
            {
                if (Guid.TryParse(parameters.tenant_id, out Guid tenantId))
                    AddCriteria(c => c.TenantId == tenantId);
            }

            if (Guid.TryParse(parameters.organization_id, out Guid id))
                AddCriteria(c => c.OrganizationId == id);

            if (parameters.page_size > 0)
            {
                var page = parameters.page == 0 ? parameters.page : parameters.page - 1;
                ApplyPaging(page * parameters.page_size, parameters.page_size);
            }
        }
    }
}