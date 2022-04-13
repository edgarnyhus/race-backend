using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Queries.Helpers;

namespace Application.Interfaces;

public interface ISignpostService
{
    Task<IEnumerable<SignpostDto>> GetSignposts(QueryParameters queryParameters);
    Task<SignpostDto> GetSignpostById(string id);
    Task<SignpostDto> CreateSignpost(SignpostContract waypointContract);
    Task<bool> UpdateSignpost(string id, SignpostContract waypointContract);
    Task<bool> DeleteSignpost(string id);
    List<KeyValuePair<int, string>> GetSignpostStates();
}