﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories.Helpers;
using System.Collections.ObjectModel;

namespace Infrastructure.Data.Repositories
{
    public class OrganizationRepository : Repository<Organization>, IOrganizationRepository
    {
        private readonly IMapper _mapper;
        private readonly ILogger<OrganizationRepository> _logger;
        private readonly IServiceScopeFactory _scopeFactory;


        public OrganizationRepository(LocusBaseDbContext dbContext, IServiceScopeFactory scopeFactory, IMapper mapper, ILogger<OrganizationRepository> logger)
            : base(dbContext, mapper, logger)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<IEnumerable<Organization>> Find(ISpecification<Organization> specification)
        {
            var query = _dbContext.Set<Organization>()
                // Include customer tree (Children=Organiztions)- 5 levels by default
                .Include(i => i.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children)
                .AsNoTracking();

            var result = SpecificationEvaluator<Organization>.GetQuery(query, specification, true);
            return await result.ToListAsync();
        }

        public async Task<Organization> FindById(string id)
        {
            try
            {
                Organization result;

                Guid guid;
                bool isValidId = Guid.TryParse(id, out guid);

                var query = _dbContext.Set<Organization>()
                    // Show 5 levels
                    .Include(i => i.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children)
                    .AsNoTracking();

                if (isValidId)
                    result = await query.FirstOrDefaultAsync(x => x.Id == guid);
                else
                    result = await query
                        .FirstOrDefaultAsync(x => x.CustomerNumber == id ||
                        x.OrganizationNumber == id);

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError($"Find Exception {e}", e.ToString());
                throw;
            }
        }

        public override async Task<Organization> Add(Organization entity)
        {
            Organization parent = null;
            parent = await _dbContext.Set<Organization>()
                .Include(i => i.Children)
                .FirstOrDefaultAsync(x => x.Id == entity.ParentId);
            if (parent != null)
                entity.Level = parent.Level + 1;
            else
                entity.Level = 0;

            var result = await _dbContext.Set<Organization>().AddAsync(entity);

            if (parent == null)
            {
                // Add to tenant.Children collection
                var tenant = await _dbContext.Set<Tenant>()
                    .Include(i => i.Children)
                    .FirstOrDefaultAsync(x => x.Id == entity.TenantId);
                if (tenant != null)
                {
                    if (tenant.Children == null)
                        tenant.Children = new Collection<Organization>();
                    tenant.Children.Add(entity);
                }
            }
            else
            {
                // Add to Organization.Children collection
                if (parent.Children == null)
                    parent.Children = new Collection<Organization>();
                parent.Children.Add(entity);
            }

            await _dbContext.SaveChangesAsync();

            await UpdateLevelAsync(entity, entity.Level + 1);

            return result.Entity;
        }

        public async Task<bool> Update(string id, Organization entity)
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<Organization, Organization>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)));
            var mapper = configuration.CreateMapper();

            Organization existingEntity;
            bool isValidId = Guid.TryParse(id, out Guid guid);

            var query = _dbContext.Set<Organization>()
                    .Include(i => i.Parent)
                    .Include(i => i.Children)
                    .AsNoTracking();

            if (isValidId)
                existingEntity = await query
                    .FirstOrDefaultAsync(x => x.Id == guid);
            else
                existingEntity = await query
                    .FirstOrDefaultAsync(x => x.CustomerNumber == id || x.OrganizationNumber == id);

            if (existingEntity == null)
            {
                var result = await Add(entity);
                return result != null ? true : false;
            }

            if (existingEntity.Parent != null)
                entity.Level = existingEntity.Parent.Level + 1;
            else
                entity.Level = 0;

            //mapper.Map(entity, existingEntity);
            _dbContext.Update(entity);

            //if (existingEntity.Parent?.Id != entity.ParentId)
            //{
            //    var parent = await _dbContext.Set<Organization>()
            //        .Include(i => i.Children)
            //        .FirstOrDefaultAsync(x => x.Id == entity.ParentId);

            //}

            await _dbContext.SaveChangesAsync();

            await UpdateLevelAsync(entity, entity.Level + 1);

            return true;
        }

        public async Task<bool> Remove(string id)
        {
            Organization existingEntity;
            bool isValidId = Guid.TryParse(id, out Guid guid);

            var query = _dbContext.Set<Organization>()
                    .Include(i => i.Children);

            if (isValidId)
                existingEntity = await query
                    .FirstOrDefaultAsync(x => x.Id == guid);
            else
                existingEntity = await query
                    .FirstOrDefaultAsync(x => x.CustomerNumber == id);

            if (existingEntity == null)
                return false;

            // Traverse Children to set new Parent
            var parentId = existingEntity.ParentId;
            IEnumerable<Organization> children = existingEntity.Children;
            foreach (var item in existingEntity.Children.Reverse())
            {
                existingEntity.Children.Remove(item);
                item.ParentId = parentId;
                item.Level = existingEntity.Level;
                await UpdateLevelAsync(item, existingEntity.Level);
            }
            existingEntity.ParentId = null;

            _dbContext.Remove(existingEntity);
            await _dbContext.SaveChangesAsync();

            return true;
        }


        private async Task UpdateLevelAsync(Organization entity, int level)
        {
            if (entity.Children == null)
                return;

            foreach (var item in entity.Children)
            {
                var e = await _dbContext.Set<Organization>()
                    .Include(i => i.Children)
                    .FirstOrDefaultAsync(c => c.Id == item.Id);
                e.Level = level;
                if (e.Children.Count > 0)
                    await UpdateLevelAsync(e, level + 1);
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}
