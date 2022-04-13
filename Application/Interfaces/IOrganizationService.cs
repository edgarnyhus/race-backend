using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Queries.Helpers;

namespace Application.Interfaces
{
    public interface IOrganizationService
    {
        Task<IEnumerable<OrganizationDto>> GetOrganizations(QueryParameters queryParameters);
        Task<OrganizationDto> GetOrganizationById(string id);
        Task<OrganizationDto> CreateOrganization(OrganizationContract contract);
        Task<bool> UpdateOrganization(string id, OrganizationContract contract);
        Task<bool> DeleteOrganization(string id);
        Task<bool> HasParent(string id);
    }
}