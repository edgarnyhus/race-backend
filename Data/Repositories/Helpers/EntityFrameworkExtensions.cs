// using Microsoft.EntityFrameworkCore;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;
// using System.Linq;
// using System.Linq.Expressions;
// using System.Reflection;
//
// namespace Data.Repositories.Helpers
// {
//     public static class EntityFrameworkExtensions
//     {
//         public static void ValidateEntities(this DbContext context)
//         {
//             var validationErrors = new List<DbEntityValidationError>();
//             var entries = context.ChangeTracker.Entries<IValidatableObject>()
//                 .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
//
//             foreach (var e in entries)
//             {
//                 var errors = e.Entity.Validate(null);
//                 if (errors.Any()) validationErrors.Add(new DbEntityValidationError(e, errors));
//             }
//
//             if (validationErrors.Any()) throw new DbEntityValidationException(validationErrors);
//         }        
//
// // Used to update entities and avoid 'The instance of entity type 'X' cannot be tracked because another instance with the same key value for {'Id'} is already being tracked.' error
//         public static void UpdateEntity<T>(
//             this DbContext context,
//             T existingEntity,
//             T newEntity,
//             Type[] typesToIgnore = null,
//             IEnumerable<Expression<Func<T, object>>> propertiesToIgnore = null
//         ) where T : class
//         {
//             using (var objectUpdater = new EfObjectUpdater())
//             {
//                 objectUpdater.UpdateEntity(context, existingEntity, newEntity, 
//                     typesToIgnore, propertiesToIgnore?.Select(p => p.GetPropertyFromExpression()).ToArray());
//             }
//         }
//     }
//
//     internal class EfObjectUpdater : IDisposable
//     {
//         private IEnumerable<string> GetIdPropertyNames(DbContext context, object entity)
//         {
//             if (entity == null) throw new ArgumentException("Parameter cannot be null.");
//             var entityType = entity.GetType();
//
//             return context.Model
//                 .FindEntityType(entityType)
//                 .FindPrimaryKey().Properties
//                 .Select(p => p.Name)
//                 .ToList();
//         }
//
//         private IEnumerable<object> GetIdPropertyValues(DbContext context, object obj)
//         {
//             if (obj == null) throw new ArgumentException("Parameter cannot be null.");
//             var objType = obj.GetType();
//             var result = new List<object>();
//
//             foreach (var idPropertyName in GetIdPropertyNames(context, obj))
//             {
//                 result.Add(objType.GetProperty(idPropertyName).GetValue(obj));
//             }
//
//             if (Attribute.IsDefined(objType, typeof(PersistentAttribute))) return result;
//             return result.All(p => p == null || p.Equals(0)) ? Enumerable.Repeat((object)null, result.Count).ToList() : result;
//         }
//
//         private static bool PropertiesAreEqual(string propertyName, object obj1, object obj2)
//         {
//             if (obj1 == obj2) return true;
//             if (obj1 == null || obj2 == null) return false;
//             var obj1Property = obj1.GetType().GetProperty(propertyName);
//             var obj2Property = obj2.GetType().GetProperty(propertyName);
//             return obj1Property?.GetValue(obj1)?.Equals(obj2Property?.GetValue(obj2)) ?? false;
//         }
//
//         private object Find(DbContext context, IEnumerable list, object objectToFind)
//         {
//             if (list == null || objectToFind == null) throw new ArgumentException("Parameters cannot be null.");
//             var objectToFindPropertyNames = GetIdPropertyNames(context, objectToFind);
//             return list.Cast<object>().SingleOrDefault(listObject => objectToFindPropertyNames.All(p => PropertiesAreEqual(p, objectToFind, listObject)));
//         }
//
//         private void UpdateProperties(IEnumerable<string> propertyNames, object existingEntity, object newEntity)
//         {
//             bool ValuesAreEqual(object v1, object v2) => v1 == v2 || (v1?.Equals(v2) ?? false);
//             var entityType = existingEntity.GetType();
//             foreach (var propertyName in propertyNames)
//             {
//                 var prop = entityType.GetProperty(propertyName);
//                 if (prop == null) continue;
//                 var existingEntityValue = prop.GetValue(existingEntity);
//                 var newEntityValue = prop.GetValue(newEntity);
//                 if (ValuesAreEqual(existingEntityValue, newEntityValue)) continue;
//                 prop.SetValue(existingEntity, newEntityValue);
//             }
//         }
//
//         private readonly IDictionary<object, bool> _visited = new Dictionary<object, bool>();
//         private bool WasVisited(object obj)
//         {
//             return _visited.ContainsKey(obj);
//         }
//
//         private void MarkAsVisited(object obj)
//         {
//             _visited.Add(obj, true);
//         }
//
//         public void UpdateEntity<T>(DbContext context, T existingEntity, T newEntity, Type[] typesToIgnore = null, PropertyInfo[] propertiesToIgnore = null) where T : class
//         {
//             if (existingEntity == null || newEntity == null) throw new NullReferenceException();
//             var existingEntityType = existingEntity.GetType();
//             var newEntityType = newEntity.GetType();
//             if (existingEntityType != newEntityType) throw new InvalidOperationException();
//
//             if (typesToIgnore?.Contains(existingEntityType) ?? false) return;
//             if (WasVisited(existingEntity)) return;
//             MarkAsVisited(existingEntity);
//
//             var basicProperties = existingEntityType.GetProperties().Where(p =>
//                 !(propertiesToIgnore?.Contains(p) ?? false) && p.IsBasicProperty()
//             ).Select(p => p.Name).ToList();
//             var collectionProperties = existingEntityType
//                 .GetProperties()
//                 .Where(p =>
//                     basicProperties.All(bp => bp != p.Name) &&
//                     p.PropertyType
//                         .GetInterfaces()
//                         .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)) &&
//                     p.CustomAttributes.All(ca => ca.AttributeType != typeof(IgnoreMappingAttribute))
//                 )
//                 .ToList();
//             var navigationProperties = existingEntityType
//                 .GetProperties()
//                 .Where(p =>
//                     basicProperties.All(bp => bp != p.Name) &&
//                     collectionProperties.All(cp => cp.Name != p.Name) &&
//                     p.CustomAttributes.All(ca => ca.AttributeType != typeof(IgnoreMappingAttribute))
//                 ).ToList();
//
//             //basic properties
//             UpdateProperties(basicProperties, existingEntity, newEntity);
//
//             //collection properties
//             foreach (var collectionProperty in collectionProperties)
//             {
//                 var existingCollection = (IList)collectionProperty.GetValue(existingEntity);
//                 if (existingCollection == null)
//                 {
//                     var collectionType = collectionProperty.PropertyType.GenericTypeArguments.FirstOrDefault();
//                     if (collectionType != null)
//                     {
//                         existingCollection = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(collectionType));
//                     }
//                     else
//                     {
//                         existingCollection = new List<object>();
//                     }
//                     collectionProperty.SetValue(existingEntity, existingCollection);
//                 }
//                 var newCollection = ((IList)collectionProperty.GetValue(newEntity))?.Cast<object>().ToList() ?? new List<object>();
//                 var existingCollectionType = existingCollection.Cast<object>().FirstOrDefault()?.GetType();
//                 var newCollectionType = newCollection.FirstOrDefault()?.GetType();
//                 if (typesToIgnore?.Contains(existingCollectionType) ?? false) continue;
//                 if (typesToIgnore?.Contains(newCollectionType) ?? false) continue;
//
//                 foreach (var existingElement in existingCollection.Cast<object>().ToList())
//                 {
//                     if (WasVisited(existingElement)) continue;
//                     var elementExistsInTheNewList = Find(context, newCollection, existingElement) != null;
//                     if (elementExistsInTheNewList) continue;
//                     if (Attribute.IsDefined(existingCollectionType, typeof(PersistentAttribute)))
//                     {
//                         existingCollection.Remove(existingElement);
//                     }
//                     else
//                     {
//                         SetEntityState(context, existingElement, EntityState.Deleted);
//                     }
//                 }
//                 var additionList = new List<object>();
//                 foreach (var newElement in newCollection)
//                 {
//                     object existingElementInDbSet = null;
//                     try
//                     {
//                         var dbSet = GetDbSet(context, newCollectionType);
//                         existingElementInDbSet = InvokeMethod(dbSet, "Find",
//                             GetIdPropertyValues(context, newElement).ToArray());
//                     }
//                     catch (InvalidOperationException)
//                     {
//                         var dbSet = GetDbSet(context, newCollectionType);
//                         var allElements = ((IEnumerable)dbSet).ToObjectList();
//
//                         existingElementInDbSet = Find(context, allElements, newElement);
//                     }
//                     if (existingElementInDbSet == null)
//                     {
//                         UpdateEntity(context, newElement, newElement, typesToIgnore, propertiesToIgnore);
//                         additionList.Add(newElement);
//                     }
//                     else
//                     {
//                         UpdateEntity(context, existingElementInDbSet, newElement, typesToIgnore, propertiesToIgnore);
//                         var existingElement = Find(context, existingCollection, newElement);
//                         if (existingElement == null)
//                         {
//                             additionList.Add(existingElementInDbSet);
//                         }
//                         else
//                         {
//                             existingCollection[existingCollection.IndexOf(existingElement)] = existingElementInDbSet;
//                         }
//                     }
//                 }
//                 //Because the above UpdateEntiy call is recursive, if we add the new elemnets (with ID=0)
//                 //beforehand, only the first one will get to be added to the list (the other ones are with the same ID and misidentified)
//                 foreach (var o in additionList)
//                 {
//                     existingCollection.Add(o);
//                 }
//             }
//
//             //navigation properties
//             foreach (var navigationProperty in navigationProperties)
//             {
//                 if (propertiesToIgnore?.Any(p => p.Equals(navigationProperty)) ?? false) continue;
//                 if (typesToIgnore?.Contains(navigationProperty.PropertyType) ?? false) continue;
//                 var newEntityPropertyValue = navigationProperty.GetValue(newEntity);
//                 var propertyEntityDbSet = GetDbSet(context, navigationProperty.PropertyType);
//
//                 if (newEntityPropertyValue == null)
//                 {
//                     if (Attribute.IsDefined(navigationProperty.PropertyType, typeof(PersistentAttribute)))
//                     {
//                         navigationProperty.SetValue(existingEntity, null);
//                     }
//                     else
//                     {
//                         var existingPropertyValue = navigationProperty.GetValue(existingEntity);
//                         if (existingPropertyValue != null)
//                             SetEntityState(context, existingPropertyValue, EntityState.Deleted);
//                     }
//                 }
//                 else
//                 {
//                     var existingEntityPropertyValue = InvokeMethod(propertyEntityDbSet, "Find", GetIdPropertyValues(context, newEntityPropertyValue).ToArray());
//                     if (existingEntityPropertyValue == null)
//                     {
//                         navigationProperty.SetValue(existingEntity, newEntityPropertyValue);
//                     }
//                     else
//                     {
//                         navigationProperty.SetValue(existingEntity, existingEntityPropertyValue);
//                         UpdateEntity(context, existingEntityPropertyValue, newEntityPropertyValue, typesToIgnore, propertiesToIgnore);
//                     }
//                 }
//             }
//         }
//
//         private static object GetDbSet(DbContext context, Type type)
//         {
//             return context.GetType().GetMethod("Set").MakeGenericMethod(type).Invoke(context, new object[0]);
//         }
//
//         private static object InvokeMethod(object dbSet, string methodName, object[] parameters)
//         {
//             return dbSet.GetType().GetMethod(methodName).Invoke(dbSet, new object[] { parameters });
//         }
//
//         private static void SetEntityState(DbContext context, object obj, EntityState state)
//         {
//             context.Entry(obj).State = state;
//         }
//
//         public void Dispose()
//         {
//             _visited.Clear();
//         }
//     }
// }