using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Repositories.Helpers;
using Location = Domain.Models.Location;

namespace Infrastructure.Data.Context
{
    public class DbInitializer : IDbInitializer
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DbInitializer(IServiceScopeFactory scopeFactory)
        {
            this._scopeFactory = scopeFactory;
        }

        public void Initialize()
        {
            using (var serviceScope = _scopeFactory.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<RaceBackendDbContext>())
                {
                    try
                    {
                        var dbExists = context.Database.CanConnect();
                        List<string> pendingMigrations = new List<string>();
                        if (dbExists)
                            context.Database.GetPendingMigrations();
                        if (!dbExists || pendingMigrations.Count() > 0)
                            context.Database.Migrate();
                    }
                    catch (Exception)
                    {
                        // ignore..
                    }
                }
            }
        }

        public async void SeedData()
        {
            using (var serviceScope = _scopeFactory.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<RaceBackendDbContext>())
                {
                    var v = await context.SignTypes.FirstOrDefaultAsync();
                    if (v == null)
                    //if (!context.ContainerTypes.)
                    {
                        var containerType = new SignType
                        {
                            Id = new Guid("33F1DE21-B5EE-4617-9A4C-98E38CBDA445"),
                            Name = "B-0140",
                            Description = "Waste container 140l",
                        };
                        context.SignTypes.Add(containerType);

                        containerType = new SignType
                        {
                            Id = new Guid("F2B20013-18E4-4ABB-B1D2-CE8664CBCF1F"),
                            Name = "B-0240",
                            Description = "Waste container 240l",
                        };
                        context.SignTypes.Add(containerType);

                    }
                }
            }
        }
    }
}