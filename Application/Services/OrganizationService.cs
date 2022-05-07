using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Application.Helpers;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Interfaces;
using Domain.Models;
using Domain.Multitenant;
using Domain.Queries.Helpers;
using Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly TenantAccessService<Tenant> _tenantAccessService;
        private readonly IOrganizationRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<Organization> _logger;
        private readonly bool _multitenancy = false;

        public OrganizationService(TenantAccessService<Tenant> tenantAccessService, IRepository<Organization> repository,
            IConfiguration config, IMapper mapper, IMediator mediator, ILogger<Organization> logger)
        {
            _tenantAccessService = tenantAccessService;
            _repository = (IOrganizationRepository) repository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
            var value = config["Multitenancy:Enabled"];
            bool.TryParse(value, out _multitenancy);
        }

        public async Task<IEnumerable<OrganizationDto>> GetOrganizations(QueryParameters queryParameters)
        {
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);

            if (await _tenantAccessService.IsGlobalAdministrator())
            {
                queryParameters.tenant_id = null;
                queryParameters.organization_id = null;
            }
            else if (!(await _tenantAccessService.IsAdministrator()))
                queryParameters.organization_id = null;
            else
                throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");

            var result = await _repository.Find(new GetOrganizationsSpecification(queryParameters));
            var response = _mapper.Map<IEnumerable<Organization>, IEnumerable<OrganizationDto>>(result);

            return response;
        }

        public async Task<OrganizationDto> GetOrganizationById(string id)
        {
            var result = await _repository.FindById(id);
            var response = _mapper.Map<Organization, OrganizationDto>(result);

            return response;
        }

        public async Task<OrganizationDto> CreateOrganization(OrganizationContract contract)
        {
            //var isAdmin = await _tenantAccessService.IsAdministrator();
            //if (_multitenancy && !isAdmin)
            //    throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");

            var entity = _mapper.Map<OrganizationContract, Organization>(contract);
            await UpdateProperties(entity);

            entity = await _repository.Add(entity);
            var result = _mapper.Map<Organization, OrganizationDto>(entity);

            return result;
        }

        public async Task<bool> UpdateOrganization(string id, OrganizationContract contract)
        {
            //var isAdmin = await _tenantAccessService.IsAdministrator();
            //if (_multitenancy && !isAdmin)
            //    throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");

            var entity = _mapper.Map<OrganizationContract, Organization>(contract);
            await UpdateProperties(entity);
            
            var result = await _repository.Update(id, entity);

            return result;
        }

        public async Task<bool> DeleteOrganization(string id)
        {
            //var isAdmin = await _tenantAccessService.IsAdministrator();
            //if (_multitenancy && !isAdmin)
            //    throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");

            var result = await _repository.Remove(id);
            return result;
        }

        private async Task UpdateProperties(Organization entity)
        {
            Guid? tenantId = entity.TenantId;
            if (tenantId == null || tenantId == Guid.Empty)
            {
                var tenant = await _tenantAccessService.GetTenantAsync();
                tenantId = tenant?.TenantId;
            }
            entity.TenantId = tenantId;
            entity.Id = GuidExtensions.CheckGuid(entity.Id);
        }

        private string GetOrganizationIdFromRequestPath()
        {
            string organizationId = null;
            var path = _tenantAccessService.GetRequestPath();
            if (!string.IsNullOrEmpty(path))
            {
                var list = path.Split('/');
                organizationId = list[3];
            }
            return organizationId;
        }

        public async Task<bool> HasParent(string id)
        {
            var result = await _repository.FindById(id);
            return result?.ParentId != null ? true : false;
        }
    }
}
