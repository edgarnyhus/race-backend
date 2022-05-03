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
    public class RaceRepository : Repository<Race>, IRaceRepository
    {
        private readonly IMapper _mapper;
        private readonly ILogger<RaceRepository> _logger;

        public RaceRepository(RaceBackendDbContext dbContext, IMapper mapper, ILogger<RaceRepository> logger) : base(dbContext, mapper, logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<Race> Add(Race entity)
        {
            try
            {
                //await PropertyChecks.CheckPrincipleAndDependant(_dbContext, entity, entity.Organization);
                var result = await _dbContext.Set<Race>().AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                return result.Entity;
            }
            catch (Exception e)
            {
                var error = e.Message;
                if (e.InnerException != null)
                    error = e.InnerException.Message;
                _logger.LogError($"Add Race Exception: {error}");
                throw;
            }
        }

        public override async Task<bool> Update(Guid id, Race entity)
        {
            var existingEntity = await _dbContext.Set<Race>()
                .Where(e => id == e.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (existingEntity == null)
            {
                var assignment = await Add(entity);
                return assignment != null;
            }
            
            //await PropertyChecks.CheckPrincipleAndDependant(_dbContext, entity, entity.Organization);
            _mapper.Map(entity, existingEntity);

            if (IsDevelopment)
                foreach (var entry in _dbContext.ChangeTracker.Entries())
                    Console.WriteLine($"{entry.Metadata.Name}, {entry.State}");

            _dbContext.Update(existingEntity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public override async Task<bool> Remove(Guid id)
        {
            try
            {
                var result = await Task.Run(() => base.Remove(id));
                return result;
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                if (ex.InnerException != null)
                    error = ex.InnerException.Message;
                _logger.LogError($"Remove Exception: {error}");
                throw new Exception(error);
            }
        }
    }
}
