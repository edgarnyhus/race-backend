using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories.Helpers;

namespace Infrastructure.Data.Repositories
{
    public class SentinelRepository : Repository<Sentinel>, ISentinelRepository
    {
        public SentinelRepository(LocusBaseDbContext dbContext, IMapper mapper, ILogger<Repository<Sentinel>> logger) : base(dbContext, mapper, logger)
        {
        }


        public override async Task<IEnumerable<Sentinel>> Find(ISpecification<Sentinel> specification)
        {
            var result = await _dbContext.Set<Sentinel>()
                .Include(o => o.Organization)
                .Include(x => x.Race)
                .AsNoTracking()
                .ToListAsync();
            return result;
        }

        public override async Task<Sentinel> FindById(Guid id)
        {
            var result = await _dbContext.Set<Sentinel>()
                .Include(o => o.Organization)
                .Include(x => x.Race)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
            return result;
        }

        public override async Task<Sentinel> Add(Sentinel entity)
        {
            try
            {
                await PropertyChecks.CheckProperties(_dbContext, entity, null);

                var result = await _dbContext.Set<Sentinel>().AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                return result.Entity;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public override async Task<bool> Update(Guid id, Sentinel entity)
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<Sentinel, Sentinel>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)));
            var mapper = configuration.CreateMapper();

            Sentinel existingEntity = await _dbContext.Set<Sentinel>()
                .Include(x => x.Organization)
                .Include(x => x.Race)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (existingEntity == null)
            {
                var driver = await Add(entity);
                return driver != null;
            }

            await PropertyChecks.CheckPrincipleAndDependant(_dbContext, entity, entity.Organization);
            entity.Race = await PropertyChecks.CheckNavigationProperty(_dbContext, entity, entity.Race, existingEntity?.Race);

            mapper.Map(entity, existingEntity);

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public override async Task<bool> Remove(Guid id)
        {
            Sentinel entry = await _dbContext.Set<Sentinel>().Where(i => i.Id == id).SingleOrDefaultAsync();
            if (entry == null) return false;
            _dbContext.Remove(entry);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}