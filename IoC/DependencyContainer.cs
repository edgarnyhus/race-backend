using Application.Interfaces;
using Application.Services;
using Infrastructure.Data.Context;
using Domain.Interfaces;
using Infrastructure.Data.Repositories;
using Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.IoC
{
    public class DependencySignGroup
    {
        public static void RegisterServices(IServiceCollection services)
        {
            // Domain InMemory MediatR

            // Domain Handlers

            // Application layer
            services.AddScoped<IDriverService, DriverService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IRaceService, RaceService>();
            services.AddScoped<ISignGroupService, SignGroupService>();
            services.AddScoped<ISignService, SignService>();
            services.AddScoped<ISignTypeService, SignTypeService>();
            services.AddScoped<ITenantService, TenantService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserSettingsService, UserSettingsService>();
            services.AddScoped<IWaypointService, WaypointService>();

            // Data layer
            services.AddTransient<IRepository<Driver>, DriverRepository>();
            services.AddTransient<IRepository<Organization>, OrganizationRepository>();
            services.AddTransient<IRepository<Race>, RaceRepository>();
            services.AddTransient<IRepository<SignGroup>, SignGroupRepository>();
            services.AddTransient<IRepository<Sign>, SignRepository>();
            services.AddTransient<IRepository<SignType>, SignTypeRepository>();
            services.AddTransient<IRepository<Tenant>, TenantRepository>();
            services.AddTransient<IRepository<User>, UserRepository>();
            services.AddTransient<IRepository<UserSettings>, UserSettingsRepository>();
            services.AddTransient<IRepository<Waypoint>, WaypointRepository>();

            services.AddTransient<LocusBaseDbContext>();
        }
    }
}
