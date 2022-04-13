using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Repositories;

public class SignpostRepository : Repository<Signpost>, ISignpostRepository
{
    private readonly IMapper _mapper;
    private readonly ILogger<SignpostRepository> _logger;

    public SignpostRepository(RaceBackendDbContext dbContext, IMapper mapper, ILogger<SignpostRepository> logger) :
        base(dbContext, mapper, logger)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public override async Task<IEnumerable<Signpost>> Find(ISpecification<Signpost> specification)
    {
        var result = await _dbContext.Set<Signpost>()
            .Include(x => x.Sign)
            .Include(x => x.Race)
            .AsNoTracking()
            .ToListAsync();

        return result;
    }

    public override async Task<Signpost> FindById(Guid id)
    {
        var query = await _dbContext.Set<Signpost>()
            .Include(x => x.Sign)
            .Include(x => x.Race)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        return query;
    }

    public override async Task<Signpost> Add(Signpost entity)
    {
        try
        {
            var result = await _dbContext.Set<Signpost>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public override async Task<bool> Update(Guid id, Signpost entity)
    {
        var existingEntity = await _dbContext.Set<Signpost>().FirstOrDefaultAsync(a => a.Id == id);
        if (existingEntity == null)
        {
            var waypoint = await Add(entity);
            return waypoint != null;
        }

        existingEntity.Sign = entity.Sign;
        existingEntity.Location = entity.Location;
        existingEntity.Notes = entity.Notes;
        existingEntity.LastScanned = entity.LastScanned;
        existingEntity.LastScannedBy = entity.LastScannedBy;
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

    public List<KeyValuePair<int, string>> GetSignpostStates()
    {
        var result = Enum.GetValues(typeof(SignState))
            .Cast<int>()
            .ToDictionary(ee => (int) ee, ee => Enum.GetName(typeof(SignState), ee)).ToList();

        return result;
    }
}