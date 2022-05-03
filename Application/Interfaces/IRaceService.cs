using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Models;
using Domain.Queries.Helpers;

namespace Application.Interfaces
{
    public interface IRaceService
    {
        Task<IEnumerable<RaceDto>> GetAllRaces(QueryParameters queryParameters);
        Task<RaceDto> GetRaceById(string id);
        Task<RaceDto> CreateRace(RaceContract contract);
        Task<bool> UpdateRace(string id, RaceContract contract);
        Task<bool> DeleteRace(string id);
    }
}