using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Data.Context;

namespace Infrastructure.Data.Repositories.Helpers
{
    public class PropertyChecks
    {
        public static async Task<EntityBase> CheckProperties(LocusBaseDbContext context, EntityBase entity, EntityBase existingEntity)
        {
            var props = entity.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.Name == "Location" && existingEntity != null
                     && (entity.GetType() == typeof(Sign) || entity.GetType() == typeof(Waypoint)))
                {
                    var location = (Location) prop.GetValue(entity);
                    var existingLocation = (Location) prop.GetValue(existingEntity);

                    if (location != null) {
                        if (location.Id == null && existingLocation != null)
                            location.Id = existingLocation.Id;
                        prop.SetValue(entity, location);
                    }
                }
                var propertyType = prop.PropertyType;
                if (TypeExtensions.InheritsFrom(propertyType, typeof(EntityBase)))
                {
                    dynamic value = prop.GetValue(entity);
                    if (value != null)
                    {
                        // Check if its a one-to-many relationship; i.e the dependant entity will define a one-to-many property as for instance
                        //   public Guid? OrganizationId { get; set; }
                        //   public Organization? Organization { get; set; }
                        // So, it there is an OrganizationId property, it is a one-to-many relationship.
                        var principleId = props.FirstOrDefault(x => x.Name == prop.Name + "Id");
                        if (principleId != null)
                            await CheckPrincipleAndDependant(context, entity, value);
                        else
                        {
                            dynamic existingValue = null;
                            if (existingEntity != null)
                                existingValue = prop.GetValue(existingEntity);

                            if (value is Driver)
                                value = await CheckProperties(context, value, existingValue);

                            value = await CheckNavigationProperty(context, entity, value, existingValue);
                            prop.SetValue(entity, value);
                        }
                    }
                }
            }
            return entity;
        }

        // Navigation property in one-to-one relationship
        public static async Task<T> CheckNavigationProperty<T>(LocusBaseDbContext context, EntityBase entity, T navigationProperty, T existingNavigationProperty) where T : EntityBase
        {
            MapperConfiguration config = new MapperConfiguration(options =>
            {
                options.CreateMap<T, T>()
                    .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            });
            var mapper = config.CreateMapper();

            if (navigationProperty != null)
            {
                try
                {
                    var entry = await context.Set<T>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == navigationProperty.Id);

                    if (entry != null)
                    {
                        await CheckOneToOneRelationship(context, entity, navigationProperty, existingNavigationProperty);
                        mapper.Map(navigationProperty, existingNavigationProperty);

                        SetForeignKey(entity, ref navigationProperty);
                        navigationProperty = null;

                        context.Entry(existingNavigationProperty).State = EntityState.Modified;
                    }
                    else
                    {
                        if (existingNavigationProperty != null)
                        {
                            var existingEntry = await context.Set<T>()
                                .AsNoTracking()
                                .FirstOrDefaultAsync(p => p.Id == existingNavigationProperty.Id);
                            if (existingEntry != null)
                            {
                                await CheckOneToOneRelationship(context, entity, navigationProperty, existingNavigationProperty);
                            }
                        }
                        SetForeignKey(entity, ref navigationProperty);
                        var result = await context.Set<T>().AddAsync(navigationProperty);
                        navigationProperty = result.Entity;
                        context.Entry(navigationProperty).State = EntityState.Added;
                    }
                }
                catch (Exception e)
                {
                    var error = e.Message;
                    if (e.InnerException != null)
                        error = e.InnerException.Message;
                    Console.WriteLine($"CheckNavigationProperty Exeption: {error}");
                }

                return navigationProperty;
            }
            //else if (existingNavigationProperty != null)
            //{
            //    context.Entry(existingNavigationProperty).State = EntityState.Deleted;
            //}

            return null;
        }

        public static async Task<EntityBase> CheckPrincipleAndDependant<T>(LocusBaseDbContext context, EntityBase entity, T property) where T : EntityBase
        {
            if (property != null)
            {
                try
                {
                    await CheckOneToManyRelationship(context, entity, property);

                    var principle = await context.Set<T>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == property.Id);
                    if (principle == null)
                        return property;

                    Type entityType = entity.GetType();
                    Type propertyType = property.GetType();
                    PropertyInfo prop = entityType.GetProperty(propertyType.Name);
                    if (prop != null)
                        prop.SetValue(entity, null);
                    prop = entityType.GetProperty(propertyType.Name + "Id");
                    if (prop != null)
                        prop.SetValue(entity, principle.Id);

                    //UntrackEntry<T>(context, property.Id);
                    //context.Entry(principle).State = EntityState.Detached;
                }
                catch (Exception e) {
                    var error = e.Message;
                    if (e.InnerException != null)
                        error = e.InnerException.Message;
                    Console.WriteLine($"CheckPrincipleAndDependant Exeption: {error}");
                }
            }
            
            return entity;
        }

        public static async Task<Guid?> CheckOneToManyRelationship<T>(LocusBaseDbContext context, EntityBase entity, T property) where T : EntityBase
        {
            if (property != null)
            {
                try
                {
                    if (property.Id == null)
                        return entity.Id;

                    Type entityType = entity.GetType();
                    Type propertyType = property.GetType();
                    PropertyInfo prop = entityType.GetProperty(propertyType.Name + "Id");
                    if (prop == null)
                        return null;

                    var fk = (Guid?)prop.GetValue(entity);
                    if (fk != null && fk != property.Id)
                    {
                        prop.SetValue(entity, null);
                        await context.SaveChangesAsync();
                    }

                    return entity.Id;
                }
                catch (Exception e)
                {
                    var error = e.Message;
                    if (e.InnerException != null)
                        error = e.InnerException.Message;
                    Console.WriteLine($"CheckOneToManyRelationship Exeption: {error}");
                }
            }

            return null;
        }
        
        ///
        //Removing relationships link
        //Delete the principal link
        //Deleting the principal will ensure that the action specified by the Referential Constraint Action
        //enumeration will be enforced.
        //For required relationships, the dependents will all be deleted.
        //If the relationship is optional, the foreign key values of the dependents will be set to null.
        ///
        public static async Task<Guid?> CheckOneToOneRelationship<T>(LocusBaseDbContext context, EntityBase entity, T property, T existingProperty) where T : EntityBase
        {
            try
            {
                if (property == null || existingProperty == null)
                    return property?.Id;

                Type entityType = entity.GetType();
                Type propertyType = property.GetType();
                var prop = propertyType.GetProperty(entityType.Name + "Id");
                if (prop != null && property.Id != existingProperty.Id)
                {
                    Guid? id = null;
                    var value = prop.GetValue(existingProperty);
                    if (value != null)
                        id = (Guid)value;

                    if (id != null)
                    { 
                        prop.SetValue(existingProperty, null);
                        await context.SaveChangesAsync();
                    }
                }

                UntrackEntry<T>(context, existingProperty.Id);
                return property.Id;
            }
            catch (Exception e)
            {
                var error = e.Message;
                if (e.InnerException != null)
                    error = e.InnerException.Message;
                Console.WriteLine($"CheckOneToOneRelationship Exeption: {error}");
            }

            return null;
        }

        public static bool IsTracked<T>(LocusBaseDbContext context, Guid? id) where T : EntityBase
        {
            bool tracking = context.ChangeTracker.Entries<T>().Any(x => x.Entity.Id == id);
            return tracking;
        }

        public static void UntrackEntry<T>(LocusBaseDbContext context, Guid? id) where T : EntityBase
        {
            if (id != null)
            {
                bool tracking = context.ChangeTracker.Entries<T>().Any(x => x.Entity.Id == id);
                if (tracking)
                {
                    var tee = context.ChangeTracker.Entries<T>()
                        .FirstOrDefault(x => x.Entity.Id == id);
                    var tEntity = tee?.Entity;
                    if (tEntity != null)
                        context.Entry(tEntity).State = EntityState.Detached;
                }
            }
        }

        private static bool SetForeignKey<T>(EntityBase parentEntity, ref T entry)
        {
            try
            {
                if (parentEntity == null || entry == null)
                    return false;
                var id = parentEntity.Id;
                var keyName = parentEntity.GetType().Name + "Id";
                var result = SetKey(ref entry, keyName, id);
                return result;
            }
            catch (Exception e)
            {
                var error = e.Message;
                if (e.InnerException != null)
                    error = e.InnerException.Message;
                Console.WriteLine($"SetForeignKey Exeption: {error}");
            }

            return false;
        }

        public static bool SetKey<T>(ref T entry, string name, Guid? fk)
        {
            try
            {
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var pi in properties)
                {
                    if (pi.Name.Equals(name))
                    {
                        pi.SetValue(entry, fk);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                var error = e.Message;
                if (e.InnerException != null)
                    error = e.InnerException.Message;
                Console.WriteLine($"SetKey Exeption: {error}");
            }
            return false;
        }
    }
}
