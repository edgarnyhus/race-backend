using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Queries.Helpers;

namespace Application.Interfaces
{
    public interface ISentinelService
    {
        Task<IEnumerable<SentinelDto>> GetSentinels(QueryParameters queryParameters);
        Task<SentinelDto> GetSentinelById(string id);
        Task<SentinelDto> CreateSentinel(SentinelContract contract);
        Task<bool> UpdateSentinel(string id, SentinelContract contract);
        Task<bool> DeleteSentinel(string id);
    }
}