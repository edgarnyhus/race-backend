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

        }

        public GetSignsSpecification(IQueryParameters parameters) : base(parameters)
        {
            if (!string.IsNullOrEmpty(parameters.within_radius))
            {
                var wd = JsonConvert.DeserializeObject<WithinDistance>(parameters.within_radius);
                var location = new Point(wd.longitude, wd.latitude) { SRID = 4326 };

                AddCriteria(c => c.Location != null && c.GeoLocation != null &&
                                 (c.GeoLocation.Distance(location) < wd.radius));
            }
            else if (!string.IsNullOrEmpty(parameters.within_square))
            {
                var we = JsonConvert.DeserializeObject<WithinEnvelope>(parameters.within_square);
                AddCriteria(c => c.Location != null && c.GeoLocation != null);
            }

            if (parameters.multitenancy)
            {
                Guid.TryParse(parameters.tenant_id, out Guid tenantId);
                AddCriteria(c => c.TenantId == tenantId);
            }

            if (Guid.TryParse(parameters.organization_id, out Guid orgId))
                AddCriteria(c => c.OrganizationId == orgId);

            if (Guid.TryParse(parameters.signtype_id, out Guid signtypeId))
                AddCriteria(c => c.SignTypeId == signtypeId);

            if (Guid.TryParse(parameters.race_id, out Guid raceId))
                AddCriteria(c => c.RaceId == raceId);

            if (Guid.TryParse(parameters.signgroup_id, out Guid grouId))
                AddCriteria(c => c.SignGroupId == grouId);

            if (!string.IsNullOrEmpty(parameters.name))
            {
                AddCriteria(c => c.Name.StartsWith(parameters.name));
                AddCriteria(c => c.State != SignState.Discarded);
            }

            if (!string.IsNullOrEmpty(parameters.qr_code))
            {
                var qrCode = parameters.qr_code;
                AddCriteria(c => c.QrCode == qrCode);
            }

            if (!string.IsNullOrEmpty(parameters.race_day))
            {
                try
                {
                    var race_day = int.Parse(parameters.race_day);
                    AddCriteria(c => c.RaceDay == race_day);
                }
                catch (Exception) { } 
            }

            if (!string.IsNullOrEmpty(parameters.state))
            {
                var state = SignState.Unknown;
                try
                {
                    state =(SignState)int.Parse(parameters.state);
                }
                catch (Exception)
                {
                    throw new ArgumentException("Invalid 'state' parameter. Use the 'key' retrieved from api/signs/states.");
                }
                AddCriteria(c => c.State == state);
            }

            if (parameters.page_size > 0)
            {
                var page = parameters.page == 0 ? parameters.page : parameters.page - 1;
                ApplyPaging(page * parameters.page_size, parameters.page_size);
            }
        }
    }
}
