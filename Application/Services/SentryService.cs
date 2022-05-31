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
    public class SentinelService : ISentinelService
    {
        private readonly TenantAccessService<Tenant> _tenantAccessService;
        private readonly ISentinelRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<SentinelService> _logger;
        private readonly bool _multitenancy = false;

        public SentinelService(TenantAccessService<Tenant> tenantAccessService, IRepository<Sentinel> repository, IMapper mapper,
            ILogger<SentinelService> logger, IConfiguration config)
        {
            _tenantAccessService = tenantAccessService;
            _repository = (ISentinelRepository)repository;
            _mapper = mapper;
            _logger = logger;
            var value = config["Multitenancy:Enabled"];
            bool.TryParse(value, out _multitenancy);
        }

        public async Task<IEnumerable<SentinelDto>> GetSentinels(QueryParameters queryParameters)
        {
            var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
            await tenantValidation.Validate(queryParameters);

            var result = await _repository.Find(new GetSentinelsSpecification(queryParameters));
            var response = _mapper.Map<IEnumerable<Sentinel>, IEnumerable<SentinelDto>>(result);
            return response;
        }

        public async Task<SentinelDto> GetSentinelById(string id)
        {
            Guid.TryParse(id, out Guid guid);
            var result = await _repository.FindById(guid);
            var response = _mapper.Map<Sentinel, SentinelDto>(result);
            return response;
        }

        public async Task<SentinelDto> CreateSentinel(SentinelContract contract)
        {
            var entity = await UpdateProperties(contract);
            
            var result = await _repository.Add(entity);

            var response = _mapper.Map<Sentinel, SentinelDto>(result);
            return response;
        }

        public async Task<bool> UpdateSentinel(string id, SentinelContract contract)
        {
            var entity = await UpdateProperties(contract);

            Guid.TryParse(id, out Guid guid);
            var result  = await _repository.Update(guid,entity);
            return result;
        }

        public async Task<bool> DeleteSentinel(string id)
        {
            Guid.TryParse(id, out Guid guid);
            var result = await _repository.Remove(guid);
            return result;
        }


        private async Task<Sentinel> UpdateProperties(SentinelContract contract)
        {
            var entity = _mapper.Map<SentinelContract, Sentinel>(contract);

            Guid? tenantId = entity.TenantId;
            if (tenantId == null || tenantId == Guid.Empty)
            {
                var tenant = await _tenantAccessService.GetTenantAsync();
                tenantId = tenant?.TenantId;
                entity.TenantId = tenantId;
            }
            entity.Id = GuidExtensions.CheckGuid(entity.Id);
            return entity;
        }
    }
}