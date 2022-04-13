using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Repositories;

public class WaypointRepository : Repository<Waypoint>, IWaypointRepository
{
    private readonly IMapper _mapper;
    private readonly ILogger<WaypointRepository> _logger;

    public WaypointRepository(RaceBackendDbContext dbContext, IMapper mapper, ILogger<WaypointRepository> logger) :
        base(dbContext, mapper, logger)
    {
        _mapper = mapper;
        _logger = logger;
    }


    public override async Task<IEnumerable<Waypoint>> Find(ISpecification<Waypoint> specification)
    {
        var result = await _dbContext.Set<Waypoint>()
            .AsNoTracking()
            .ToListAsync();

        return result;
    }

    public override async Task<Waypoint> FindById(Guid id)
    {
        var query = await _dbContext.Set<Waypoint>()
            .Include(x => x.Race)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        return query;
    }

    public override async Task<Waypoint> Add(Waypoint entity)
    {
        try
        {
            var result = await _dbContext.Set<Waypoint>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public override async Task<bool> Update(Guid id, Waypoint entity)
    {
        var existingEntity = await _dbContext.Set<Waypoint>().FirstOrDefaultAsync(a => a.Id == id);
        if (existingEntity == null)
        {
            var waypoint = await Add(entity);
            return waypoint != null;
        }

        existingEntity.Location = entity.Location;
        existingEntity.Notes = entity.Notes;
        existingEntity.Race = entity.Race;
        existingEntity.RaceId = entity.RaceId;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public override async Task<bool> Remove(Guid id)
    {
        Waypoint entry = await _dbContext.Set<Waypoint>().Where(i => i.Id == id).SingleOrDefaultAsync();
        if (entry == null) return false;
        _dbContext.Remove(entry);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}