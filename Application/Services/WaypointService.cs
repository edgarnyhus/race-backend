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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class WaypointService : IWaypointService
{
    private readonly IWaypointRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TenantAccessService<Tenant> _tenantAccessService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<WaypointService> _logger;
    private readonly bool _multitenancy = false;

    public WaypointService(IHttpContextAccessor httpContextAccessor, TenantAccessService<Tenant> tenantAccessService,
        IRepository<Waypoint> repository, IRepository<User> userRepository,
        IMapper mapper, IMediator mediator, ILogger<WaypointService> logger, IConfiguration config)
    {
        _httpContextAccessor = httpContextAccessor;
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

        // Get RaceId from request path
        var path = _httpContextAccessor.HttpContext.Request.Path.Value;
        var arr = path.Split('/');
        queryParameters.race_id = arr[3];

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
        //var isAdmin = await _tenantAccessService.IsAdministrator();
        //if (_multitenancy && !isAdmin)
        //    throw new UnauthorizedAccessException(
        //        "Unauthorized. You are missing the necessary permissions to issue this request.");

        var entity = _mapper.Map<WaypointContract, Waypoint>(waypointContract);
        //entity.Id = GuidExtensions.CheckGuid(entity.Id);
        entity.Location.Timestamp = DateTime.UtcNow;

        if (entity.RaceId == null)
        {
            // Get RaceId from request path - api/races/{race_id}/waypoints
            var path = _httpContextAccessor.HttpContext.Request.Path.Value;
            var arr = path.Split('/');
            entity.RaceId = new Guid(arr[3]);
        }

        var result = await _repository.Add(entity);
        var response = _mapper.Map<Waypoint, WaypointDto>(result);
        return response;
    }

    public async Task<bool> UpdateWaypoint(string id, WaypointContract waypointContract)
    {
        //var isAdmin = await _tenantAccessService.IsAdministrator();
        //if (_multitenancy && !isAdmin)
        //    throw new UnauthorizedAccessException(
        //        "Unauthorized. You are missing the necessary permissions to issue this request.");

        var entity = _mapper.Map<WaypointContract, Waypoint>(waypointContract);

        if (entity.RaceId == null)
        {
            // Get RaceId from request path - api/races/{race_id}/waypoints
            var path = _httpContextAccessor.HttpContext.Request.Path.Value;
            var arr = path.Split('/');
            entity.RaceId = new Guid(arr[3]);
        }

        Guid.TryParse(id, out Guid guid);
        var result = await _repository.Update(guid, entity);
        return result;
    }

    public async Task<bool> DeleteWaypoint(string id)
    {
        //var isAdmin = await _tenantAccessService.IsAdministrator();
        //if (_multitenancy && !isAdmin)
        //    throw new UnauthorizedAccessException(
        //        "Unauthorized. You are missing the necessary permissions to issue this request.");

        Guid.TryParse(id, out Guid guid);
        var result = await _repository.Remove(guid);
        return result;
    }
}