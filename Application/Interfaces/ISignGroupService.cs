using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Queries.Helpers;

namespace Application.Interfaces;

public interface ISignGroupService
{
    Task<IEnumerable<SignGroupDto>> GetSignGroups(QueryParameters queryParameters);
    Task<SignGroupDto> GetSignGroupById(string id);
    Task<SignGroupDto> CreateSignGroup(SignGroupContract contract);
    Task<bool> UpdateSignGroup(string id, SignGroupContract contract);
    Task<bool> DeleteSignGroup(string id);
}