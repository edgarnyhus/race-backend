using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

namespace Application.Services
{
    public class TenantService : ITenantService
    {
        private readonly TenantAccessService<Tenant> _tenantAccessService;
        private readonly ITenantRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<TenantService> _logger;
        private readonly bool _multitenancy = false;

        public TenantService(TenantAccessService<Tenant> tenantAccessService, IRepository<Tenant> repository,
            IConfiguration config, IMapper mapper, IMediator mediator, ILogger<TenantService> logger)
        {
            _tenantAccessService = tenantAccessService;
            _repository = (ITenantRepository) repository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
            var value = config["Multitenancy:Enabled"];
            bool.TryParse(value, out _multitenancy);
        }

        public async Task<IEnumerable<TenantDto>> GetTenants(QueryParameters queryParameters)
        {
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);

            if (!(await _tenantAccessService.IsAdministrator()))
                throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");

            var result = await _repository.Find(new GetTenantsSpecification(queryParameters));
            var response = _mapper.Map<IEnumerable<Tenant>, IEnumerable<TenantDto>>(result);
            return response;
        }
        
        public async Task<TenantDto> GetTenantById(string id)
        {
            Guid.TryParse(id, out Guid guid);
            var result = await _repository.FindById(guid);
            var response = _mapper.Map<Tenant, TenantDto>(result);
            return response;
        }

        public async Task<TenantDto> CreateTenant(TenantContract contract)
        {
            //if (!(await _tenantAccessService.IsAdministrator()))
            //    throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");

            var entity = _mapper.Map<TenantContract, Tenant>(contract);
            UpdateProperties(entity);

            var result = await _repository.Add(entity);
            var response = _mapper.Map<Tenant, TenantDto>(result);
            return response;
        }

        public async Task<bool> UpdateTenant(string id, TenantContract contract)
        {
            //if (!(await _tenantAccessService.IsAdministrator()))
            //    throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");

            var entity = _mapper.Map<TenantContract, Tenant>(contract);
            Guid.TryParse(id, out Guid guid);
            var result = await _repository.Update(guid, entity);
            return result;
        }

        public async Task<bool> DeleteTenant(string id)
        {
            //if (!(await _tenantAccessService.IsAdministrator()))
            //    throw new UnauthorizedAccessException("Unauthorized. You are missing the necessary permissions to issue this request.");

            Guid.TryParse(id, out Guid guid);
            var result = await _repository.Remove(guid);
            return result;
        }


        private void UpdateProperties(Tenant entity)
        {
            entity.Id = GuidExtensions.CheckGuid(entity.Id);
        }
    }
}