using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Repositories;

public class SignGroupRepository : Repository<SignGroup>, ISignGroupRepository
{
    private readonly IMapper _mapper;
    private readonly ILogger<SignGroupRepository> _logger;

    public SignGroupRepository(LocusBaseDbContext dbContext, IMapper mapper, ILogger<SignGroupRepository> logger) :
        base(dbContext, mapper, logger)
    {
        _mapper = mapper;
        _logger = logger;
    }

}