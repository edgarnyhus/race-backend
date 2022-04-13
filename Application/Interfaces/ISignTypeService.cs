using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Queries.Helpers;

namespace Application.Interfaces
{
	public interface ISignTypeService
	{
        Task<IEnumerable<SignTypeDto>> GetSignTypes(QueryParameters queryParameters);
        Task<SignTypeDto> GetSignTypeById(string id);
        Task<SignTypeDto> CreateSignType(SignTypeContract containerTypeContract);
        Task<bool> UpdateSignType(string id, SignTypeContract containerTypeContract);
        Task<bool> DeleteSignType(string id);
        Task<int> GetCount(QueryParameters queryParameters);
    }
}

