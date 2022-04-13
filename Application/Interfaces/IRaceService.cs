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
        Task<IEnumerable<RaceDto>> GetAllRoutes(QueryParameters queryParameters);
        Task<RaceDto> GetRouteById(string id);
        Task<RaceDto> CreateRoute(RaceContract contract);
        Task<bool> UpdateRoute(string id, RaceContract contract);
        Task<bool> DeleteRoute(string id);
    }
}