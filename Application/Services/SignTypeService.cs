using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Helpers;
using Application.Interfaces;
using AutoMapper;
using Domain.Contracts;
using Domain.Dtos;
using Domain.Interfaces;
using Domain.Models;
using Domain.Multitenant;
using Domain.Queries.Helpers;
using Domain.Specifications;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class SignTypeService : ISignTypeService
{
    private readonly TenantAccessService<Tenant> _tenantAccessService;
    private readonly ISignTypeRepository _repository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<SignTypeService> _logger;
    private readonly bool _multitenancy = false;

    public SignTypeService(TenantAccessService<Tenant> tenantAccessService, IRepository<SignType> repository, IMapper mapper,
        IMediator mediator, ILogger<SignTypeService> logger, IConfiguration config)
    {
        _tenantAccessService = tenantAccessService;
        _repository = (ISignTypeRepository) repository;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
        var value = config["Multitenancy:Enabled"];
        bool.TryParse(value, out _multitenancy);
    }

    public async Task<IEnumerable<SignTypeDto>> GetSignTypes(QueryParameters queryParameters)
    {
        //var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
        //await tenantValidation.Validate(queryParameters);

        var result = await _repository.Find(new GetSignTypesSpecification(queryParameters));
        var response = _mapper.Map<IEnumerable<SignType>, IEnumerable<SignTypeDto>>(result);
        return response;
    }

    public async Task<SignTypeDto> GetSignTypeById(string id)
    {
        Guid.TryParse(id, out Guid guid);
        var result = await _repository.FindById(guid);
        var response = _mapper.Map<SignType, SignTypeDto>(result);
        return response;
    }

    public async Task<SignTypeDto> CreateSignType(SignTypeContract contract)
    {
        var entity = _mapper.Map<SignTypeContract, SignType>(contract);
        var result = await _repository.Add(entity);

        var response = _mapper.Map<SignType, SignTypeDto>(result);
        return response;
    }

    public async Task<bool> UpdateSignType(string id, SignTypeContract contract)
    {
        var entity = _mapper.Map<SignTypeContract, SignType>(contract);
        Guid.TryParse(id, out Guid guid);
        var result = await _repository.Update(guid, entity);
        return result;
    }

    public async Task<bool> DeleteSignType(string id)
    {
        Guid.TryParse(id, out Guid guid);
        var result = await _repository.Remove(guid);
        return result;
    }

    public async Task<int> GetCount(QueryParameters queryParameters)
    {
        var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
        await tenantValidation.Validate(queryParameters);
        queryParameters.page_size = 0;

        var result = _repository.Count(new GetSignTypesSpecification(queryParameters));
        return result;
    }

}