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
    public class DriverRepository : Repository<Driver>, IDriverRepository
    {
        public DriverRepository(RaceBackendDbContext dbContext, IMapper mapper, ILogger<Repository<Driver>> logger) : base(dbContext, mapper, logger)
        {
        }


        public override async Task<IEnumerable<Driver>> Find(ISpecification<Driver> specification)
        {
            var result = await _dbContext.Set<Driver>()
                .Include(o => o.Organization)
                .Include(x => x.Race)
                .AsNoTracking()
                .ToListAsync();
            return result;
        }

        public override async Task<Driver> FindById(Guid id)
        {
            var result = await _dbContext.Set<Driver>()
                .Include(o => o.Organization)
                .Include(x => x.Race)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);
            return result;
        }

        public override async Task<Driver> Add(Driver entity)
        {
            try
            {
                await PropertyChecks.CheckProperties(_dbContext, entity, null);

                var result = await _dbContext.Set<Driver>().AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                return result.Entity;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public override async Task<bool> Update(Guid id, Driver entity)
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<Driver, Driver>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)));
            var mapper = configuration.CreateMapper();

            Driver existingEntity = await _dbContext.Set<Driver>()
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
            Driver entry = await _dbContext.Set<Driver>().Where(i => i.Id == id).SingleOrDefaultAsync();
            if (entry == null) return false;
            _dbContext.Remove(entry);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}