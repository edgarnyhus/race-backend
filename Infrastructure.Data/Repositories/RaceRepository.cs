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
using Domain.Specifications;


namespace Infrastructure.Data.Repositories
{
    public class RaceRepository : Repository<Race>, IRaceRepository
    {
        private readonly IMapper _mapper;
        private readonly ILogger<RaceRepository> _logger;

        public RaceRepository(LocusBaseDbContext dbContext, IMapper mapper, ILogger<RaceRepository> logger) : base(dbContext, mapper, logger)
        {
            _mapper = mapper;
            _logger = logger;
        }


        public override async Task<Race> Add(Race entity)
        {
            await PropertyChecks.CheckProperties(_dbContext, entity, null);

            entity = await base.Add(entity);
            return entity;
        }

        public async Task<bool> Update(string id, Race entity)
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<Race, Race>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)));
            var mapper = configuration.CreateMapper();

            if (!Guid.TryParse(id, out Guid guid))
                return false;

            var existingEntity = await _dbContext.Set<Race>()
                .Include(x => x.Signs)
                .Include(x => x.Waypoints)
                .SingleOrDefaultAsync(x => x.Id == guid);

            if (existingEntity == null)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;

                entity = await Add(entity);
                return entity != null;
            }

            await PropertyChecks.CheckProperties(_dbContext, entity, existingEntity);
            mapper.Map(entity, existingEntity);

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public override async Task<bool> Remove(Guid id)
        {
            //if (!Guid.TryParse(id, out Guid guid))
            //    return false;

            var entity = await _dbContext.Set<Race>()
                .Include(i => i.Signs).ThenInclude(l => l.Location)
                .Include(i => i.Waypoints).ThenInclude(l => l.Location)
                .SingleOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return false;

            var signs = await _dbContext.Signs.Where(x => x.RaceId == entity.Id).ToListAsync();
            foreach (var sign in signs)
                await RemoveSignFromRace(sign.Id.ToString());

            _dbContext.Set<Race>().Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }


        //
        // Race Signs
        // 

        public async Task<IEnumerable<Sign>> GetSignsOfRace(GetSignsSpecification specification)
        {
            Guid.TryParse(specification.Parameters.race_id, out Guid guid);
            var result = _dbContext.Set<Sign>()
                .Where(x => x.RaceId == guid)
                .Include(i => i.SignType)
                .Include(i => i.Location)
                .AsNoTracking()
                .AsEnumerable();

            return await Task.Run(() => result.ToList());
        }

        public async Task<bool> AddSignToRace(Sign entity)
        {
            var sign = await _dbContext.Signs
                .Where(x => x.Id == entity.Id)
                .FirstOrDefaultAsync();

            if (sign == null)
                throw new ArgumentException("Invalid request. A sign with the given Id does not exist.");
            if (entity.RaceId == null)
                throw new ArgumentException("Invalid request. A race withh the specified race_id does not exist.");
            if (sign.RaceDay != entity.RaceDay)
                throw new ArgumentException("Invalid property (race_day). Mismatch between race.race_day and sign.race_day.");

            await PropertyChecks.CheckProperties(_dbContext, entity, sign);
            sign.RaceId = entity.RaceId;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSignInRace(string id, Sign entity)
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<Sign, Sign>()
                    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)));
            var mapper = configuration.CreateMapper();

            var query = _dbContext.Set<Sign>()
                .Include(x => x.Location);

            Sign existingEntity = null;
            if (Guid.TryParse(id, out Guid guid))
                existingEntity = await query
                    .SingleOrDefaultAsync(x => x.Id == guid);
            else
            {
                // QR Code may be on the format '1|UPZYUM70U6VT' where suffix separated by '|' is race day
                var raceDay = entity.RaceDay;
                var qrCode = id;
                var arr = id.Split('|');
                if (arr.Count() > 1)
                {
                    try { raceDay = int.Parse(arr[0]); } catch (Exception) { };
                    if (raceDay != entity.RaceDay)
                        throw new ArgumentException("Mismatch between sign.race_day and id (<race_day>|<qr_code>).");
                    qrCode = arr[1];
                }
                existingEntity = await query
                    .SingleOrDefaultAsync(x => x.QrCode == qrCode && x.RaceDay == entity.RaceDay);
            }

            if (existingEntity == null)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;

                var sign = await AddSignToRace(entity);
                return sign != null;
            }

            if (existingEntity.RaceDay != entity.RaceDay)
                throw new ArgumentException("Invalid property (race_day).");

            await PropertyChecks.CheckProperties(_dbContext, entity, existingEntity);
            mapper.Map(entity, existingEntity);

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveSignFromRace(string id)
        {
            Sign entity = null;

            var query = _dbContext.Set<Sign>()
                .Include(i => i.Location);

            if (Guid.TryParse(id, out Guid guid))
                entity = await query
                    .Where(x => x.Id == guid)
                    .FirstOrDefaultAsync();
            else
            {
                // QR Code may be on the format '1|UPZYUM70U6VT' where suffix separated by '|' is race day
                var raceDay = 1;
                var qrCode = id;
                var arr = id.Split('|');
                if (arr.Count() > 1)
                {
                    try { raceDay = int.Parse(arr[0]); } catch (Exception) { };
                    qrCode = arr[1];
                }
                entity = await query
                        .Where(x => x.QrCode == qrCode && x.RaceDay == raceDay)
                        .FirstOrDefaultAsync();
            }

            if (entity == null)
                return false;

            entity.RaceId = null;
            entity.Location = null;
            entity.State = entity.State == SignState.Discarded ? SignState.Discarded : SignState.Inactive;

            await _dbContext.SaveChangesAsync();
            return true;
        }


        public async Task CheckIfRaceDaySignsExist(Race race)
        {
            var numberOfRaceDays = race.RaceDay;
            var signs = await _dbContext.Signs.ToListAsync();
            foreach (var sign in signs)
            {
                if (sign.State != SignState.Discarded)
                {
                    if (sign.RaceDay == 0)
                    {
                        sign.RaceDay = 1;
                        _dbContext.Signs.Update(sign);
                        await _dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        var item = signs.Find(x => x.QrCode == sign.QrCode && x.RaceDay == race.RaceDay);
                        if (item == null)
                        {
                            item = new Sign();
                            item = sign;
                            item.Id = null;
                            item.RaceDay = race.RaceDay;
                            item.RaceId = null;
                            item.Location = null;
                            item.GeoLocation = null;
                            item.LastScanned = null;
                            item.LastScannedBy = null;
                            item.State = SignState.Inactive;

                            await _dbContext.Signs.AddAsync(item);
                            await _dbContext.SaveChangesAsync();
                        }
                    }

                }
            }
        }
    }
}
