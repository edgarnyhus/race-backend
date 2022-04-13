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

public class SignpostService : ISignpostService
{
    private readonly ISignpostRepository _repository;
    private readonly TenantAccessService<Tenant> _tenantAccessService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<SignpostService> _logger;
    private readonly bool _multitenancy = false;

    public SignpostService(TenantAccessService<Tenant> tenantAccessService, IRepository<Signpost> repository,
        IRepository<User> userRepository,
        IMapper mapper, IMediator mediator, ILogger<SignpostService> logger, IConfiguration config)
    {
        _tenantAccessService = tenantAccessService;
        _repository = repository as ISignpostRepository;
        _userRepository = (IUserRepository) userRepository;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
        var value = config["Multitenancy:Enabled"];
        bool.TryParse(value, out _multitenancy);
    }

    public async Task<IEnumerable<SignpostDto>> GetSignposts(QueryParameters queryParameters)
    {
        var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
        await tenantValidation.Validate(queryParameters);

        var result = await _repository.Find(new GetSignpostsSpecification(queryParameters));
        var response = _mapper.Map<IEnumerable<Signpost>, IEnumerable<SignpostDto>>(result);
        return response;
    }

    public async Task<SignpostDto> GetSignpostById(string id)
    {
        Guid.TryParse(id, out Guid guid);
        var result = await _repository.FindById(guid);
        var response = _mapper.Map<Signpost, SignpostDto>(result);

        return response;
    }

    public async Task<SignpostDto> CreateSignpost(SignpostContract waypointContract)
    {
        var isAdmin = await _tenantAccessService.IsAdministrator();
        if (_multitenancy && !isAdmin)
            throw new UnauthorizedAccessException(
                "Unauthorized. You are missing the necessary permissions to issue this request.");

        var entity = _mapper.Map<SignpostContract, Signpost>(waypointContract);
        var result = await _repository.Add(entity);
        var response = _mapper.Map<Signpost, SignpostDto>(result);
        return response;
    }

    public async Task<bool> UpdateSignpost(string id, SignpostContract waypointContract)
    {
        var isAdmin = await _tenantAccessService.IsAdministrator();
        if (_multitenancy && !isAdmin)
            throw new UnauthorizedAccessException(
                "Unauthorized. You are missing the necessary permissions to issue this request.");

        var entity = _mapper.Map<SignpostContract, Signpost>(waypointContract);
        Guid.TryParse(id, out Guid guid);
        var result = await _repository.Update(guid, entity);
        return result;
    }

    public async Task<bool> DeleteSignpost(string id)
    {
        var isAdmin = await _tenantAccessService.IsAdministrator();
        if (_multitenancy && !isAdmin)
            throw new UnauthorizedAccessException(
                "Unauthorized. You are missing the necessary permissions to issue this request.");

        Guid.TryParse(id, out Guid guid);
        var result = await _repository.Remove(guid);
        return result;
    }


    public List<KeyValuePair<int, string>> GetSignpostStates()
    {
        var result = _repository.GetSignpostStates();
        return result;
    }
}