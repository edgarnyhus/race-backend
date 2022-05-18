using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;
using Infrastructure.Data.Repositories.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Repositories;

public class WaypointRepository : Repository<Waypoint>, IWaypointRepository
{
    private readonly IMapper _mapper;
    private readonly ILogger<WaypointRepository> _logger;

    public WaypointRepository(LocusBaseDbContext dbContext, IMapper mapper, ILogger<WaypointRepository> logger) :
        base(dbContext, mapper, logger)
    {
        _mapper = mapper;
        _logger = logger;
    }


    //public override async Task<IEnumerable<Waypoint>> Find(ISpecification<Waypoint> specification)
    //{
    //    var result = await _dbContext.Set<Waypoint>()
    //        .AsNoTracking()
    //        .ToListAsync();

    //    return result;
    //}

    //public override async Task<Waypoint> FindById(Guid id)
    //{
    //    var query = await _dbContext.Set<Waypoint>()
    //        .Include(x => x.Race)
    //        .AsNoTracking()
    //        .FirstOrDefaultAsync(a => a.Id == id);
    //    return query;
    //}

    public override async Task<Waypoint> Add(Waypoint entity)
    {
        try
        {
            await PropertyChecks.CheckProperties(_dbContext, entity, null);

            entity = await base.Add(entity);

            return entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public override async Task<bool> Update(Guid id, Waypoint entity)
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<Waypoint, Waypoint>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)));
        var mapper = configuration.CreateMapper();

        var existingEntity = await _dbContext.Set<Waypoint>()
            .Include(x => x.Location)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (existingEntity == null)
        {
            _dbContext.Entry(entity).State = EntityState.Detached;

            var waypoint = await Add(entity);
            return waypoint != null;
        }

        await PropertyChecks.CheckProperties(_dbContext, entity, existingEntity);
        mapper.Map(entity, existingEntity);

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public override async Task<bool> Remove(Guid id)
    {
        var entity = await _dbContext.Set<Waypoint>()
            .Include(i => i.Location)
            .Where(i => i.Id == id).SingleOrDefaultAsync();

        if (entity == null)
            return false;

        _dbContext.Remove(entity);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}