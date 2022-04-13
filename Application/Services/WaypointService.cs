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

public class WaypointService : IWaypointService
{
    private readonly IWaypointRepository _repository;
    private readonly TenantAccessService<Tenant> _tenantAccessService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<WaypointService> _logger;
    private readonly bool _multitenancy = false;

    public WaypointService(TenantAccessService<Tenant> tenantAccessService, IRepository<Waypoint> repository,
        IRepository<User> userRepository,
        IMapper mapper, IMediator mediator, ILogger<WaypointService> logger, IConfiguration config)
    {
        _tenantAccessService = tenantAccessService;
        _repository = (IWaypointRepository) repository;
        _userRepository = (IUserRepository) userRepository;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
        var value = config["Multitenancy:Enabled"];
        bool.TryParse(value, out _multitenancy);
    }

    public async Task<IEnumerable<WaypointDto>> GetWaypoints(QueryParameters queryParameters)
    {
        var tenantValidation = new TenantValidation(_tenantAccessService, _multitenancy);
        await tenantValidation.Validate(queryParameters);

        var result = await _repository.Find(new GetWaypointsSpecification(queryParameters));
        var response = _mapper.Map<IEnumerable<Waypoint>, IEnumerable<WaypointDto>>(result);
        return response;
    }

    public async Task<WaypointDto> GetWaypointById(string id)
    {
        Guid.TryParse(id, out Guid guid);
        var result = await _repository.FindById(guid);
        var response = _mapper.Map<Waypoint, WaypointDto>(result);

        return response;
    }

    public async Task<WaypointDto> CreateWaypoint(WaypointContract waypointContract)
    {
        var isAdmin = await _tenantAccessService.IsAdministrator();
        if (_multitenancy && !isAdmin)
            throw new UnauthorizedAccessException(
                "Unauthorized. You are missing the necessary permissions to issue this request.");

        var entity = _mapper.Map<WaypointContract, Waypoint>(waypointContract);
        var result = await _repository.Add(entity);
        var response = _mapper.Map<Waypoint, WaypointDto>(result);
        return response;
    }

    public async Task<bool> UpdateWaypoint(string id, WaypointContract waypointContract)
    {
        var isAdmin = await _tenantAccessService.IsAdministrator();
        if (_multitenancy && !isAdmin)
            throw new UnauthorizedAccessException(
                "Unauthorized. You are missing the necessary permissions to issue this request.");

        var entity = _mapper.Map<WaypointContract, Waypoint>(waypointContract);
        Guid.TryParse(id, out Guid guid);
        var result = await _repository.Update(guid, entity);
        return result;
    }

    public async Task<bool> DeleteWaypoint(string id)
    {
        var isAdmin = await _tenantAccessService.IsAdministrator();
        if (_multitenancy && !isAdmin)
            throw new UnauthorizedAccessException(
                "Unauthorized. You are missing the necessary permissions to issue this request.");

        Guid.TryParse(id, out Guid guid);
        var result = await _repository.Remove(guid);
        return result;
    }
}