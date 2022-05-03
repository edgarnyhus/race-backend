using System;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;

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
            var signType = parameters.sign_type;
            if (!string.IsNullOrEmpty(parameters.within_radius))
            {
                var wd = JsonConvert.DeserializeObject<WithinDistance>(parameters.within_radius);
                if (!string.IsNullOrEmpty(wd.sign_type))
                    signType = wd.sign_type;

                var location = new Point(wd.longitude, wd.latitude) { SRID = 4326 };

                AddCriteria(c => c.Location != null && c.GeoLocation != null &&
                                 (c.GeoLocation.Distance(location) < wd.radius));
            }
            else if (!string.IsNullOrEmpty(parameters.within_square))
            {
                var we = JsonConvert.DeserializeObject<WithinEnvelope>(parameters.within_square);
                if (!string.IsNullOrEmpty(we.sign_type))
                    signType = we.sign_type;

                AddCriteria(c => c.Location != null && c.GeoLocation != null);
            }

            if (parameters.multitenancy)
            {
                if (Guid.TryParse(parameters.tenant_id, out Guid tenantId))
                    AddCriteria(c => c.TenantId == tenantId);
            }

            if (Guid.TryParse(parameters.organization_id, out Guid orgId))
                AddCriteria(c => c.OrganizationId == orgId);

            if (Guid.TryParse(parameters.race_id, out Guid raceId))
                AddCriteria(c => c.RaceId == raceId);
            else if (Guid.TryParse(parameters.sign_group_id, out Guid grouId))
                AddCriteria(c => c.SignGroupId == grouId);
            else if (!string.IsNullOrEmpty(parameters.qr_code))
            {
                var qrCode = parameters.qr_code;
                AddCriteria(c => c.QrCode == qrCode);
            }
            else if (!string.IsNullOrEmpty(parameters.sign_type))
            {
                var name = parameters.sign_type;
                AddCriteria(c => c.SignType != null && name.Equals(c.SignType.Name, StringComparison.InvariantCultureIgnoreCase));
            }
            else if (!string.IsNullOrEmpty(parameters.state))
            {
                AddCriteria(c => c.State.ToString() == parameters.state);
            }

            if (parameters.page_size > 0)
            {
                var page = parameters.page == 0 ? parameters.page : parameters.page - 1;
                ApplyPaging(page * parameters.page_size, parameters.page_size);
            }
        }
    }
}
