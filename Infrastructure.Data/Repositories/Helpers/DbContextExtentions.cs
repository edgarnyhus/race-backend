using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Dapper;
using Domain.Interfaces;

namespace Infrastructure.Data.Repositories.Helpers
{
    public static class GenericGraphUpdate
    {
        public interface IObjectWithState
        {
            ObjectState ObjectState { get; set; }
        }


        public enum ObjectState
        {
            Unchanged,
            Added,
            Modified,
            Deleted
        }

        public static async void ApplyChanges<TEntity>(DbContext cntx, TEntity root)
            where TEntity : class, IObjectWithState
        {
            // using (var context = cntx)
            // {
            cntx.Set<TEntity>().Add(root);

            CheckForEntitiesWithoutStateInterface(cntx);

            foreach (var entry in cntx.ChangeTracker.Entries<IObjectWithState>())
            {
                IObjectWithState stateInfo = entry.Entity;
                entry.State = ConvertState(stateInfo.ObjectState);
                Console.WriteLine($"{entry}");
            }

            await cntx.SaveChangesAsync();
            // }
        }

        private static EntityState ConvertState(ObjectState state)
        {
            switch (state)
            {
                case ObjectState.Added:
                    return EntityState.Added;
                case ObjectState.Modified:
                    return EntityState.Modified;
                case ObjectState.Deleted:
                    return EntityState.Deleted;
                default:
                    return EntityState.Unchanged;
            }
        }

        private static void CheckForEntitiesWithoutStateInterface(DbContext context)
        {
            var entitiesWithoutState = context.ChangeTracker.Entries().Where(x => !(x.Entity is IObjectWithState));
            if (entitiesWithoutState.Any())
            {
                throw new NotSupportedException("All entities must implement IObjectWithState");
            }
        }


        public static void UpdateThroughGraph<T>(this DbContext context, T entity,
            Dictionary<string, List<string>> columnsToBeUpdated)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            context.ChangeTracker.TrackGraph(entity, e =>
            {
                string navigationPropertyName = e.Entry.Entity.GetType().Name;

                if (e.Entry.IsKeySet)
                {
                    e.Entry.State = EntityState.Unchanged;
                    e.Entry.Property("Modifieddate").CurrentValue = DateTime.Now;

                    if (columnsToBeUpdated.ContainsKey(navigationPropertyName))
                    {
                        foreach (var property in e.Entry.Properties)
                        {
                            if (columnsToBeUpdated[e.Entry.Entity.GetType().Name].Contains(property.Metadata.Name))
                            {
                                property.IsModified = true;
                            }
                        }
                    }
                }
                else
                {
                    e.Entry.State = EntityState.Added;
                }
            });
        }


        /// <summary>
        /// Get list of objects that represents the Primary Key of an entity
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static object[] GetPrimaryKeyValues(this EntityEntry entry)
        {
            return entry.Metadata.FindPrimaryKey()
                .Properties
                .Select(p => entry.Property(p.Name).CurrentValue)
                .ToArray();
        }

        /// <summary>
        /// simple update method that will help you to do a full update to an aggregate graph with all related entities in it.
        /// the update method will take the loaded aggregate entity from the DB and the passed one that may come from the API layer.
        /// the method will update just the eager loaded entities in the aggregate "The included entities"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="newEntity">The De-Attached Entity</param>
        /// <param name="existingEntity">The Attached BD Entity</param>
        public static void InsertUpdateOrDeleteGraph<T>(this DbContext context, T newEntity, T existingEntity)
            where T : class
        {
            insertUpdateOrDeleteGraph(context, newEntity, existingEntity, null);
        }


        public static void insertUpdateOrDeleteGraph<T>(this DbContext context, T newEntity, T existingEntity,
            string aggregateType) where T : class
        {
            if (existingEntity == null)
            {
                context.Add(newEntity);
            }
            else if (newEntity == null)
            {
                Console.WriteLine($"Remove property...");
                // context.Remove(existingEntity);
            }
            else
            {
                var existingEntry = context.Entry(existingEntity);
                existingEntry.CurrentValues.SetValues(newEntity);

                aggregateType = "Domain..Models.InitialOperatingHours";
                foreach (var n in existingEntry.Navigations)
                {
                    Console.WriteLine($"{n.Metadata.ClrType.FullName}, {n.IsLoaded}");
                    if (!n.IsLoaded)
                    {
                        try
                        {
                            n.Load();
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }

                var navigationProperties =
                    existingEntry.Navigations.Where(n => n.IsLoaded && n.Metadata.ClrType.FullName != aggregateType);
                foreach (var navigationEntry in navigationProperties)
                {
                    var passedNavigationObject = existingEntry.Entity.GetType()
                        .GetProperty(navigationEntry.Metadata.Name)?.GetValue(newEntity);

                    if (navigationEntry.Metadata.IsCollection)
                    {
                        // the navigation property is list
                        if (!(navigationEntry.CurrentValue is IEnumerable<object> existingNavigationObject))
                            throw new NullReferenceException(
                                $"Couldn't iterate through the DB value of the Navigation '{navigationEntry.Metadata.Name}'");

                        if (!(passedNavigationObject is IEnumerable<object> passedNavigationObjectEnumerable))
                            throw new NullReferenceException(
                                $"Couldn't iterate through the passed Navigation list of '{navigationEntry.Metadata.Name}'");

                        foreach (var newValue in passedNavigationObjectEnumerable)
                        {
                            var newId = context.Entry(newValue).GetPrimaryKeyValues();
                            var existingValue = existingNavigationObject.FirstOrDefault(v =>
                                context.Entry(v).GetPrimaryKeyValues().SequenceEqual(newId));
                            if (existingValue == null)
                            {
                                var addMethod = existingNavigationObject.GetType().GetMethod("Add");

                                if (addMethod == null)
                                    throw new NullReferenceException(
                                        $"The collection type in the Navigation property '{navigationEntry.Metadata.Name}' doesn't have an 'Add' method.");

                                addMethod.Invoke(existingNavigationObject, new[] {newValue});
                            }

                            //Update sub navigation
                            insertUpdateOrDeleteGraph(context, newValue, existingValue,
                                existingEntry.Metadata.ClrType.FullName);
                        }

                        foreach (var existingValue in existingNavigationObject.ToList())
                        {
                            var existingId = context.Entry(existingValue).GetPrimaryKeyValues();

                            if (passedNavigationObjectEnumerable.All(v =>
                                !context.Entry(v).GetPrimaryKeyValues().SequenceEqual(existingId)))
                            {
                                var addMethod = existingNavigationObject.GetType().GetMethod("Remove");

                                if (addMethod == null)
                                    throw new NullReferenceException(
                                        $"The collection type in the Navigation property '{navigationEntry.Metadata.Name}' doesn't have an 'Remove' method.");

                                addMethod.Invoke(existingNavigationObject, new[] {existingValue});
                            }
                        }
                    }
                    else
                    {
                        // the navigation is not a list
                        if (passedNavigationObject != null)
                            insertUpdateOrDeleteGraph(context, passedNavigationObject, navigationEntry.CurrentValue,
                                existingEntry.Metadata.ClrType.FullName);
                    }
                }
            }
        }
    }
}