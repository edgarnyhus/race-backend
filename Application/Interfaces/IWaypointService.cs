using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Queries.Helpers;

namespace Application.Interfaces;

public interface IWaypointService
{
    Task<IEnumerable<WaypointDto>> GetWaypoints(QueryParameters queryParameters);
    Task<WaypointDto> GetWaypointById(string id);
    Task<WaypointDto> CreateWaypoint(WaypointContract waypointContract);
    Task<bool> UpdateWaypoint(string id, WaypointContract waypointContract);
    Task<bool> DeleteWaypoint(string id);
}