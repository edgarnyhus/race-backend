using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Data.Context
{
    public class DbInitializer : IDbInitializer
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;

        public DbInitializer(IServiceScopeFactory scopeFactory, IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _config = config;
        }

        public void Initialize()
        {
            using (var serviceScope = _scopeFactory.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<LocusBaseDbContext>())
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
                using (var context = serviceScope.ServiceProvider.GetService<LocusBaseDbContext>())
                {
                    var vink = await context.Tenants.FirstOrDefaultAsync(x => x.Name.StartsWith("Vink"));
                    if (vink == null)
                    {
                        vink = new Tenant()
                        {
                            Id = new Guid("843c0cfa-dacf-46d8-8a89-20d66e107cca"),
                            Name = "Vink",
                            Description = "Vink Tenant",
                            Identifier = "vink-kort.no"
                        };

                        context.Tenants.Add(vink);
                    }

                    var locusbase = await context.Tenants.FirstOrDefaultAsync(x => x.Name.StartsWith("Locus"));
                    if (locusbase == null)
                    {
                        locusbase = new Tenant()
                        {
                            Id = new Guid("50837eca-3ef5-456f-8799-580c4a4a10fc"),
                            Name = "LocusBase",
                            Identifier = "locusbase.no",
                            Description = "LocusBase Tenant",
                        };

                        context.Tenants.Add(locusbase);
                    }
                    await context.SaveChangesAsync();

                    var organization = await context.Organizations.FirstOrDefaultAsync(x => x.Name.StartsWith("Vink"));
                    if (organization == null)
                    {
                        organization = new Organization()
                        {
                            Id = new Guid("a471808d-4dc8-4b3a-88c9-ad7e5b24ed9a"),
                            Name = "Vink AS",
                            Identifier = "vink-kort.no",
                            Level = 0,
                            TenantId = new Guid("843c0cfa-dacf-46d8-8a89-20d66e107cca")
                        };

                        context.Organizations.Add(organization);
                        vink.Children.Add(organization);
                    }

                    organization = await context.Organizations.FirstOrDefaultAsync(x => x.Name.StartsWith("Locus"));
                    if (organization == null)
                    {
                        organization = new Organization()
                        {
                            Id = new Guid("9e6af333-68df-4328-afd2-83f234b0aeed"),
                            Name = "LocusBase AS",
                            Identifier = "locusbase.no",
                            Level = 0,
                            TenantId = new Guid("50837eca-3ef5-456f-8799-580c4a4a10fc")
                        };

                        context.Organizations.Add(organization);
                        locusbase.Children.Add(organization);
                    }
                    await context.SaveChangesAsync();

                    var signType = await context.SignTypes.FirstOrDefaultAsync();
                    if (signType == null)
                    {
                        //signType = new SignType
                        //{
                        //    Id = new Guid("224ce0ea-0842-4e4e-83d4-b5720fcb15de"),
                        //    Name = "Other danger",
                        //    Description = "Other danger - bicycle race",
                        //};
                        //context.SignTypes.Add(signType);

                        signType = new SignType
                        {
                            Id = new Guid("85dff9b7-2851-488e-8dc1-469c090bb25c"),
                            Name = "302",
                            Description = "Sign 302 - No entry",
                        };
                        context.SignTypes.Add(signType);

                        //signType = new SignType
                        //{
                        //    Id = new Guid("920794a0-f089-4d5e-a5b0-6da9aa98b493"),
                        //    Name = "560",
                        //    Description = "Sign 560 - Information board",
                        //};
                        //context.SignTypes.Add(signType);

                        //signType = new SignType
                        //{
                        //    Id = new Guid("89a8ad02-0894-4ba6-9858-496b3ec689e9"),
                        //    Name = "370",
                        //    Description = "Sign 370 - No stopping",
                        //};
                        //context.SignTypes.Add(signType);

                        //signType = new SignType
                        //{
                        //    Id = new Guid("4f94bfd5-a1be-493b-8466-4d41aee293f3"),
                        //    Name = "372",
                        //    Description = "Sign 372 - No parking",
                        //};
                        //context.SignTypes.Add(signType);

                        //signType = new SignType
                        //{
                        //    Id = new Guid("f631e471-6fc6-48c3-82fb-213a14501327"),
                        //    Name = "149",
                        //    Description = "Sign 149 - Risk for queue",
                        //};
                        //context.SignTypes.Add(signType);

                        //signType = new SignType
                        //{
                        //    Id = new Guid("cfe9e77a-e16e-4542-b4a7-3ef5b1054c23"),
                        //    Name = "Direction",
                        //    Description = "Directional marker",
                        //};
                        //context.SignTypes.Add(signType);

                        signType = new SignType
                        {
                            Id = new Guid("7ee1fe29-3e68-4640-8788-aedac3665955"),
                            Name = "SafeCycling",
                            Description = "Safe Cycling",
                        };
                        context.SignTypes.Add(signType);

                        await context.SaveChangesAsync();
                    }

                    int numberOfRaceDays = 4;
                    try { numberOfRaceDays = int.Parse(_config["NumberOfRaceDays"]); } catch { }
                    var signs = await context.Signs.ToListAsync();
                    foreach (var sign in signs)
                    {
                        if (sign.State != SignState.Discarded)
                        {
                            if (sign.RaceDay == 0)
                            {
                                sign.RaceDay = 1;
                                context.Signs.Update(sign);
                                await context.SaveChangesAsync();
                            }

                            for (int raceDay = 2; raceDay <= numberOfRaceDays; raceDay++)
                            {
                                var item = await context.Signs.FirstOrDefaultAsync(x =>
                                    x.QrCode == sign.QrCode && x.RaceDay == raceDay && x.State != SignState.Discarded);
                                if (item == null)
                                {
                                    item = new Sign();
                                    item = sign;
                                    item.Id = null;
                                    item.RaceDay = raceDay;
                                    item.RaceId = null;
                                    item.Location = null;
                                    item.GeoLocation = null;
                                    item.LastScanned = null;
                                    item.LastScannedBy = null;
                                    item.State = SignState.Inactive;

                                    await context.Signs.AddAsync(item);
                                    await context.SaveChangesAsync();
                                }
                            }

                        }
                    }
                }
            }
        }
    }
}