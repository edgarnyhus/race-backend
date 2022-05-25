using System;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces;
using Domain.Models;

namespace Domain.Specifications
{
    public sealed class GetRacesSpecification : BaseSpecification<Race>
    {
        public GetRacesSpecification(string id) : base(x => x.Id.Equals(id))
        {

        }

        public GetRacesSpecification(IQueryParameters parameters) : base(parameters)
        {
            if (parameters.multitenancy)
            {
                Guid.TryParse(parameters.tenant_id, out Guid tenantId);
                AddCriteria(c => c.TenantId == tenantId);
            }

            if (Guid.TryParse(parameters.organization_id, out Guid id))
                AddCriteria(c => c.OrganizationId == id);

            if (!string.IsNullOrEmpty(parameters.scheduled_at))
            {
                var date = DateTimeOffset.Parse(parameters.scheduled_at);
                //AddCriteria(c => EF.Functions.DateDiffDay(c.ScheduledAt, date) == 0);
                AddCriteria(c => c.ScheduledAt != null &&
                    c.ScheduledAt.Value.Day == date.Day && c.ScheduledAt.Value.Month == date.Month && c.ScheduledAt.Value.Year == date.Year);
            }

            if (parameters.page_size > 0)
            {
                var page = parameters.page == 0 ? parameters.page : parameters.page - 1;
                ApplyPaging(page * parameters.page_size, parameters.page_size);
            }
        }
    }
}