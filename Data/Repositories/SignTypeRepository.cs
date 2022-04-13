using AutoMapper;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Repositories;

public class SignTypeRepository : Repository<SignType>, ISignTypeRepository
{
    private readonly IMapper _mapper;
    private readonly ILogger<SignTypeRepository> _logger;

    public SignTypeRepository(RaceBackendDbContext dbContext, IMapper mapper, ILogger<SignTypeRepository> logger)
        : base(dbContext, mapper, logger)
    {
        _mapper = mapper;
        _logger = logger;
    }

}