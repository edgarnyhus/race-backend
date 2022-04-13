using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Interfaces;
using Domain.Models;
using Domain.Models.Helpers;

namespace Domain.Multitenant
{
    public class TenantStore : ITenantStore<Tenant>
    {
        private readonly ITenantRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<TenantStore> _logger;

        public TenantStore(IRepository<Tenant> repository, IMapper mapper, IMediator mediator, ILogger<TenantStore> logger)
        {
            _repository = (ITenantRepository)repository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
        }

        
        /// Get a tenant for a given identifier
        public async Task<TenantInfo?> GetTenantAsync(string? identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return null;
            var tenant = await _repository.FindByIdentifier(identifier);
            return tenant;
        }

        public async Task<bool> HasParent(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;
            var result = await _repository.HasParent(id);
            return result;
        }

    }
}