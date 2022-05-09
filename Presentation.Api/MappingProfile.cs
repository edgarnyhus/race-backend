using System;
using AutoMapper;
using Application.Helpers;
using Domain.Dtos;
using Domain.Contracts;
using Domain.Models;

namespace Api
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Driver, Driver>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Driver, DriverDto>()
                .ReverseMap();
            CreateMap<DriverContract, Driver>()
                .ReverseMap();
            CreateMap<Location, Location>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Location, LocationDto>()
                //.ForMember(d => d.Timestamp, opt => opt.MapFrom<AttachmentCreatedDateResolver, DateTime>(s => (DateTime)s.Timestamp))
                .ReverseMap();
            CreateMap<LocationContract, Location>()
                .ReverseMap();
            CreateMap<Organization, Organization>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Organization, OrganizationDto>()
                .ReverseMap();
            CreateMap<OrganizationContract, Organization>()
                .ReverseMap();
            CreateMap<Race, Race>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Race, RaceDto>()
                .ReverseMap();
            CreateMap<RaceContract, Race>()
                .ReverseMap();
            CreateMap<Role, Role>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Role, RoleDto>()
                .ReverseMap();
            CreateMap<RoleContract, Role>()
                .ReverseMap();
            CreateMap<Sign, Sign>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Sign, SignDto>()
                .ReverseMap();
            CreateMap<SignContract, Sign>()
                .ReverseMap();
            CreateMap<SignGroup, SignGroup>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SignGroup, SignGroupDto>()
                .ReverseMap();
            CreateMap<SignGroupContract, SignGroup>()
                .ReverseMap();
            CreateMap<SignType, SignType>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<SignType, SignTypeDto>()
                .ReverseMap();
            CreateMap<SignTypeContract, SignType>()
                .ReverseMap();
            CreateMap<Tenant, Tenant>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Tenant, TenantDto>()
                .ReverseMap();
            CreateMap<TenantContract, Tenant>()
                .ReverseMap();
            CreateMap<User, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<User, UserDto>()
                .ReverseMap()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UserContract, User>()
                .ReverseMap()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UserSettings, UserSettings>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<UserSettings, UserSettingsDto>()
                .ReverseMap();
            CreateMap<UserSettingsContract, UserSettings>()
                .ReverseMap();
            CreateMap<Waypoint, Waypoint>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Waypoint, WaypointDto>()
                .ReverseMap();
            CreateMap<WaypointContract, Waypoint>()
                .ReverseMap();

            CreateMap<Identity, IdentityDto>()
                .ReverseMap();
            CreateMap<AppMetadataContract, AppMetadata>()
                .ReverseMap();
            CreateMap<AppMetadata, AppMetadataDto>()
                .ReverseMap();
            CreateMap<UserMetadataContract, UserMetadata>()
                .ReverseMap();
            CreateMap<UserMetadata, UserMetadataDto>()
                .ReverseMap();
            CreateMap<Multifactor, MultifactorDto>()
                .ReverseMap();
        }
    }
}
