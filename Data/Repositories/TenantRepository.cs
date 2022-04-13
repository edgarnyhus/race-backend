using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Domain.Interfaces;
using Domain.Models;
using Domain.Models.Helpers;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class TenantRepository : Repository<Tenant>, ITenantRepository
    {
        private readonly IMapper _mapper;
        private readonly ILogger<TenantRepository> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public TenantRepository(RaceBackendDbContext dbContext, IServiceScopeFactory scopeFactory, IMapper mapper,
            ILogger<TenantRepository> logger) : base(dbContext, mapper, logger)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<IEnumerable<Tenant>> Find(ISpecification<Tenant> specification)
        {
            var query = _dbContext.Set<Tenant>()
                // Include customer tree (Children=Organiztions)- 5 levels by default
                .Include(i => i.Children.Where(p => p.Level == 0))
                    .ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children)
                .AsNoTracking();

            var result = SpecificationEvaluator<Tenant>.GetQuery(query, specification, true);
            return await result.ToListAsync();
        }

        public override async Task<Tenant> FindById(Guid id)
        {
            var result = await _dbContext.Set<Tenant>()
                // Include customer tree (Children=Organiztions)- 5 levels by default
                .Include(i => i.Children.Where(p => p.Level == 0))
                    .ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return result;
        }


        /// 
        /// Misc
        /// 

        public async Task<TenantInfo> FindByIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return null;

            TenantInfo tenantInfo = new TenantInfo();

            // Check domain identifier
            var organizations = await _dbContext.Set<Organization>()
                .Where(e => e.Identifier.Contains(identifier))
                .AsNoTracking()
                .ToListAsync();

            if (organizations != null)
            {
                foreach (var o in organizations)
                {
                    var identifiers = o.Identifier.Split(',');
                    var result = identifiers.SingleOrDefault(c => c == identifier);
                    if (result != null)
                    {
                        tenantInfo.TenantId = o.TenantId;
                        tenantInfo.OrganizationId = o.Id;
                        break;
                    }
                }
            }

            if (tenantInfo.TenantId == null)
            {
                var tenants = await _dbContext.Set<Tenant>()
                    //.Include(i => i.Children.Where(p => p.Level == 0))
                    //    .ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children)
                    .Where(e => e.Identifier.Contains(identifier))
                    .AsNoTracking()
                    .ToListAsync();

                if (tenants != null)
                {
                    foreach (var t in tenants)
                    {
                        var identifiers = t.Identifier.Split(',');
                        var result = identifiers.SingleOrDefault(c => c == identifier);
                        if (result != null)
                        {
                            tenantInfo.TenantId = t.Id;
                            break;
                        }
                    }
                }
            }

            // If no matching identifier, try to get tenant by name
            if (tenantInfo.TenantId == null)
            {
                var tenant = await _dbContext.Set<Tenant>()
                    .Where(e => e.Name == identifier)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
                if (tenant != null)
                    tenantInfo.TenantId = tenant.Id;
            }

            return tenantInfo;
        }

        public async Task<bool> HasParent(string id)
        {
            Organization org = null;
            Guid guid;

            if (string.IsNullOrEmpty(id))
                return false;

            if (Guid.TryParse(id, out guid))
            {
                org = await _dbContext.Set<Organization>()
                    .Where(e => guid == e.Id)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }

            return org?.ParentId != null ? true : false;
        }
    }
}