using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Queries.Helpers;

namespace Application.Interfaces
{
    public interface ISignService
    {
        Task<IEnumerable<SignDto>> GetSigns(QueryParameters queryParameters);
        Task<SignDto> GetSignById(string id);
        Task<SignDto> CreateSign(SignContract contract);
        Task<bool> UpdateSign(string id, SignContract contract);
        Task<bool> DeleteSign(string id);
        Task<int> GetCount(QueryParameters queryParameters);
    }
}
