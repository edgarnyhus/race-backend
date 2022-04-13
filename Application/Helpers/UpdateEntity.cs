using System;
using System.Reflection;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using Domain.Interfaces;
using Domain.Models;
using Domain.Multitenant;
using Location = Domain.Models.Location;

namespace Application.Helpers
{
    public class UpdateEntity
    {
        private readonly TenantAccessService<Tenant> _tenantAccessService;

        public UpdateEntity(TenantAccessService<Tenant> tenantAccessService)
        {
            _tenantAccessService = tenantAccessService;
        }

        public T CheckOrganization<T>(T entity) where T : EntityBase
        {
            Type entityType = entity.GetType();
            var prop = entityType.GetProperty("OrganizationId");
            if (prop == null)
                return entity;

            var value = prop.GetValue(entity);
            if (value != null)
            {
                prop = entityType.GetProperty("Organization");
                prop?.SetValue(entity, null);
                return entity;
            }

            prop = entityType.GetProperty("Organization");
            var organization = (Organization)prop?.GetValue(entity);
            if (organization == null)
                return entity;

            prop.SetValue(entity, organization);
            return entity;
        }

        public async Task<dynamic> UpdateProperties<T>(T entity, T eEntity, IRepository<T> repository) where T : EntityBase
        {
            Location location = null;
            Type entityType = entity.GetType();
            PropertyInfo prop = entityType.GetProperty("Location");
            var value = prop?.GetValue(entity);
            if (value != null)
                location = (Location)value;
            Point geoLocation = null;
            if (location != null)
                geoLocation = new Point(location.Longitude, location.Latitude) { SRID = 4326 };

            entity = CheckOrganization(entity);

            Guid? organizationId = null;
            prop = entityType.GetProperty("OrganizationId");
            value = prop?.GetValue(entity);
            if (value != null)
                organizationId = (Guid)value;

            Guid? tenantId = null;
            prop = entityType.GetProperty("TenantId");
            value = prop?.GetValue(entity);
            if (value != null)
                tenantId = (Guid)value;

            if (tenantId == null)
            {
                var tenant = await _tenantAccessService.GetTenantAsync();
                if (tenant != null)
                {
                    tenantId = tenant.TenantId;
                    if (organizationId == null)
                        organizationId = tenant.OrganizationId;
                }
            }

            T existingEntity = eEntity;
            if (existingEntity == null && entity.Id != null)
                existingEntity = await repository.FindById((Guid)entity.Id);

            if (existingEntity == null)
            {
                if (tenantId != null)
                {
                    prop = entityType.GetProperty("TenantId");
                    prop?.SetValue(entity, tenantId);

                    prop = entityType.GetProperty("Organization");
                    value = prop?.GetValue(entity);
                    if (value != null)
                    {
                        var organization = (Organization)value;
                        organization.TenantId = tenantId;
                        prop = entityType.GetProperty("Organization");
                        prop?.SetValue(entity, organization);
                    }
                    else
                    {
                        prop = entityType.GetProperty("OrganizationId");
                        prop?.SetValue(entity, organizationId);
                    }
                }

                entity.Id = GuidExtensions.CheckGuid(entity.Id);
            }

            if (geoLocation != null)
            {
                prop = entityType.GetProperty("GeoLocation");
                prop?.SetValue(entity, geoLocation);
            }

            return entity;
        }
    }
}
