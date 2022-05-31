    
using Domain.Models;
using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Data.Context
{
    public class LocusBaseDbContext : DbContext
    {
        public LocusBaseDbContext()
        {
            
        }
        
        public LocusBaseDbContext(DbContextOptions<LocusBaseDbContext> options) : base(options)
        {
            //this.Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<Sentinel> Sentinels { get; set; }
        public DbSet<Sign> Signs { get; set; }
        public DbSet<SignGroup> SignGroups { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<SignType> SignTypes { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<Race> Races { get; set; }
        public DbSet<Waypoint> Waypoints { get; set; }


        protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder )
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dir = Directory.GetCurrentDirectory();
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("config.json")
                    .Build();
                var connectionString = configuration.GetConnectionString("LocusBasedDbConnection");
                bool.TryParse(configuration["useMySql"], out bool useMySql);
                if (useMySql)
                {
                    optionsBuilder.UseMySql(connectionString,
                        ServerVersion.AutoDetect(connectionString),
                        x => x.UseNetTopologySuite());
                }
                else
                {
                    optionsBuilder.UseSqlServer(connectionString, x => x.UseNetTopologySuite());
                }
            }
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var valueComparer = new ValueComparer<List<int>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList());

            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Race>()
                .Property(p => p.Name)
                .HasMaxLength(64);
            modelBuilder.Entity<Race>()
                .Property(p => p.LogoUrl)
                .HasMaxLength(256);
            modelBuilder.Entity<Race>()
                .Property(p => p.CreatedBy)
                .HasMaxLength(64);
            modelBuilder.Entity<Race>()
                .Property(p => p.Notes)
                .HasMaxLength(512);
            modelBuilder.Entity<Race>()
                .HasMany(p => p.Waypoints)
                .WithOne(p => p.Race)
                .HasForeignKey(f => f.RaceId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Race>()
                .HasMany(p => p.Signs)
                .WithOne(p => p.Race)
                .HasForeignKey(f => f.RaceId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Race>()
                .HasOne(p => p.Organization)
                .WithMany(b => b.Races)
                .HasForeignKey(f => f.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Waypoint>()
                .Property(p => p.Alias)
                .HasMaxLength(64);
            modelBuilder.Entity<Waypoint>()
                .Property(p => p.Notes)
                .HasMaxLength(512);
            modelBuilder.Entity<Waypoint>()
                .HasOne(p => p.Race)
                .WithMany(b => b.Waypoints)
                .HasForeignKey(f => f.RaceId);
            modelBuilder.Entity<Waypoint>()
                .HasOne(p => p.Location)
                .WithOne(b => b.Waypoint)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sign>()
                .HasIndex(p => p.Id)
                .IsUnique();
            modelBuilder.Entity<Sign>()
                .Property(p => p.Name)
                .HasMaxLength(64);
            modelBuilder.Entity<Sign>()
                .Property(p => p.Notes)
                .HasMaxLength(512);
            modelBuilder.Entity<Sign>()
                .Property(p => p.QrCode)
                .HasMaxLength(38);
            //modelBuilder.Entity<Sign>()
            //    .HasIndex(u => u.QrCode)
            //    .IsUnique();
            modelBuilder.Entity<Sign>()
                .HasOne(p => p.Organization)
                .WithMany(b => b.Signs)
                .HasForeignKey(f => f.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Sign>()
                .HasOne(p => p.SignGroup)
                .WithMany(b => b.Signs)
                .HasForeignKey(f => f.SignGroupId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Sign>()
                .Property(e => e.State)
                .HasMaxLength(32);
            modelBuilder.Entity<Sign>()
                .Property(e => e.LastScannedBy)
                .HasMaxLength(64);
            modelBuilder.Entity<Sign>()
                .HasOne(p => p.Race)
                .WithMany(b => b.Signs)
                .HasForeignKey(f => f.RaceId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Sign>()
                .HasOne(p => p.Location)
                .WithOne(b => b.Sign)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Sign>()
                .HasOne(p => p.SignType)
                .WithMany(b => b.Signs)
                .HasForeignKey(f => f.SignTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Sign>()
                .Property(p => p.State)
                .HasConversion(
                    v => v.ToString(),
                    v => (SignState)Enum.Parse(typeof(SignState), v));

            modelBuilder.Entity<SignType>()
                .Property(p => p.Name)
                .HasMaxLength(64);
            modelBuilder.Entity<SignType>()
                .Property(p => p.ImageUrl)
                .HasMaxLength(64);  
            modelBuilder.Entity<SignType>()
                .Property(p => p.Description)
                .HasMaxLength(512);
            modelBuilder.Entity<SignType>()
                .HasMany(p => p.Signs)
                .WithOne(p => p.SignType)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SignGroup>()
                .HasIndex(p => p.Id)
                .IsUnique();
            modelBuilder.Entity<SignGroup>()
                .Property(p => p.Name)
                .HasMaxLength(64);
            modelBuilder.Entity<SignGroup>()
                .Property(p => p.Notes)
                .HasMaxLength(512);
            modelBuilder.Entity<SignGroup>()
                .HasMany(p => p.Signs)
                .WithOne(p => p.SignGroup)
                .HasForeignKey(f => f.SignGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Location>()
                .Property(p => p.Address)
                .HasMaxLength(128);
            modelBuilder.Entity<Location>()
                .Property(p => p.Latitude)
                .IsRequired();
            modelBuilder.Entity<Location>()
                .Property(p => p.Longitude)
                .IsRequired();
            modelBuilder.Entity<Location>()
                .Property(p => p.Timestamp)
                .IsRequired();
            modelBuilder.Entity<Location>()
                .HasOne(p => p.Waypoint)
                .WithOne(b => b.Location);
            modelBuilder.Entity<Location>()
                .HasOne(p => p.Sign)
                .WithOne(b => b.Location);
            
            modelBuilder.Entity<Organization>()
                .HasIndex(p => p.Id)
                .IsUnique();
            modelBuilder.Entity<Organization>()
                .Property(p => p.Name)
                .HasMaxLength(64)
                .IsRequired();
            modelBuilder.Entity<Organization>()
                .Property(p => p.LogoUrl)
                .HasMaxLength(256);
            modelBuilder.Entity<Organization>()
                .Property(p => p.Identifier)
                .HasMaxLength(256);
            modelBuilder.Entity<Organization>()
                .Property(p => p.CustomerNumber)
                .HasMaxLength(64);
            modelBuilder.Entity<Organization>()
                .Property(p => p.OrganizationNumber)
                .HasMaxLength(64);

            modelBuilder.Entity<Sentinel>()
                .Property(p => p.Name)
                .HasMaxLength(64);
            modelBuilder.Entity<Sentinel>()
                .Property(p => p.PhoneNumber)
                .HasMaxLength(32);
            modelBuilder.Entity<Sentinel>()
                .Property(p => p.Email)
                .HasMaxLength(64);
            
            modelBuilder.Entity<Tenant>()
                .HasMany(p => p.Children);
            modelBuilder.Entity<Tenant>()
                .HasIndex(p => p.Id)
                .IsUnique();
            modelBuilder.Entity<Tenant>()
                .Property(p => p.Name)
                .HasMaxLength(64)
                .IsRequired();
            modelBuilder.Entity<Tenant>()
                .Property(p => p.LogoUrl)
                .HasMaxLength(256);
            modelBuilder.Entity<Tenant>()
                .Property(p => p.Description)
                .HasMaxLength(256);
            modelBuilder.Entity<Tenant>()
                .Property(p => p.Identifier)
                .HasMaxLength(256);

            modelBuilder.Entity<User>()
                .HasOne(p => p.Organization)
                .WithMany(b => b.Users)
                .HasForeignKey(f => f.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<User>()
                .HasIndex(p => p.Id)
                .IsUnique();
            modelBuilder.Entity<User>()
                .Property(p => p.UserId)
                .HasMaxLength(64);
            modelBuilder.Entity<User>()
                .Property(p => p.Name)
                .HasMaxLength(64);
            modelBuilder.Entity<User>()
                .Property(p => p.Nickname)
                .HasMaxLength(64);
            modelBuilder.Entity<User>()
                .Property(p => p.Email)
                .HasMaxLength(64);
            modelBuilder.Entity<User>()
                .Property(p => p.PhoneNumber)
                .HasMaxLength(20);
            modelBuilder.Entity<User>()
                .Ignore(p => p.EmailVerified);
            modelBuilder.Entity<User>()
                .Ignore(p => p.CreatedAt);
            modelBuilder.Entity<User>()
                .Ignore(p => p.UpdatedAt);
            modelBuilder.Entity<User>()
                .Ignore(p => p.Identities);
            modelBuilder.Entity<User>()
                .Ignore(p => p.AppMetadata);
            modelBuilder.Entity<User>()
                .Ignore(p => p.UserMetadata);
            modelBuilder.Entity<User>()
                .Ignore(p => p.Picture);
            modelBuilder.Entity<User>()
                .Ignore(p => p.LastLogin);
            modelBuilder.Entity<User>()
                .Ignore(p => p.LoginsCount);
            modelBuilder.Entity<User>()
                .Ignore(p => p.Blocked);
            modelBuilder.Entity<User>()
                .Ignore(p => p.Password);
            modelBuilder.Entity<User>()
                .Ignore(p => p.Connection);

            modelBuilder.Entity<UserSettings>()
                .HasIndex(u => u.Id)
                .IsUnique();
            modelBuilder.Entity<UserSettings>()
                .HasOne(p => p.User)
                .WithOne(b => b.UserSettings)
                .HasForeignKey<UserSettings>(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserSettings>()
                .Property(us => us.CertificationWarning)
                .IsRequired();
            modelBuilder.Entity<UserSettings>()
                .Property(us => us.Widgets)
                .IsRequired()
                .HasMaxLength(128)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Trim('[', ']').Split(',', StringSplitOptions.RemoveEmptyEntries));
                    //new ValueComparer<IList<string>>(
                    //    (c1, c2) => c1.SequenceEqual(c2),
                    //    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    //    c => c.ToList()))
            modelBuilder.Entity<UserSettings>()
                .Property(us => us.Language)
                .HasMaxLength(32)
                .IsRequired();
            
            modelBuilder.Seed();
        }
    }

    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            
        }
    }

}
