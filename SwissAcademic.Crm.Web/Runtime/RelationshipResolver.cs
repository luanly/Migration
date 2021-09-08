using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SwissAcademic.Crm.Web
{
    public static class RelationshipResolver
    {
        #region Felder

        static ConcurrentDictionary<(Type, Type, Type, string), (PropertyInfo, PropertyInfo)> _relationshipPaths = new ConcurrentDictionary<(Type, Type, Type, string), (PropertyInfo, PropertyInfo)>();
        static ConcurrentDictionary<(Type, Type, Type, string), (string, string)> _relationshipPathNames = new ConcurrentDictionary<(Type, Type, Type, string), (string, string)>();

        #endregion

        #region Methoden

        #region IsIntersectType

        //N-N Relationen
        public static bool IsIntersectType(string typename)
        {
            switch (typename)
            {
                case CrmRelationshipNames.CampusContractProduct:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region GetRelationshipPath

        public static ICrmRelationship GetDirectRelationshipPath<TSource, TTarget>(Expression<Func<TSource, object>> sourceProperty = null)
           where TTarget : CitaviCrmEntity
           where TSource : CitaviCrmEntity
        {
            string sourcePropertyName = null;
            if (sourceProperty != null)
            {
                var expr = sourceProperty.Body as MemberExpression;
                sourcePropertyName = expr.Member.Name;
            }
            var result = (IEnumerable<ICrmRelationship>)GetRelationshipPath(typeof(TSource), typeof(TTarget), sourcePropertyName: sourcePropertyName);
            return result.First();
        }

        public static IEnumerable<ICrmRelationship> GetRelationshipPath<TSource, TTarget>(Expression<Func<TSource, object>> sourceProperty = null)
            where TTarget : CitaviCrmEntity
            where TSource : CitaviCrmEntity
        {
            string sourcePropertyName = null;
            if (sourceProperty != null)
            {
                var expr = sourceProperty.Body as MemberExpression;
                sourcePropertyName = expr.Member.Name;
            }
            return (IEnumerable<ICrmRelationship>)GetRelationshipPath(typeof(TSource), typeof(TTarget), sourcePropertyName: sourcePropertyName);
        }

        public static IEnumerable<ICrmRelationship> GetRelationshipPath(Type source, Type target, Type via = null, string sourcePropertyName = null)
        {
            var path = new List<ICrmRelationship>();
            if (_relationshipPaths.TryGetValue((source, target, via, sourcePropertyName), value: out (PropertyInfo Prop1, PropertyInfo Prop2) result))
            {
                if (result.Prop1 == null)
                {
                    return null;
                }

                path.Add((ICrmRelationship)result.Prop1.GetValue(Activator.CreateInstance(result.Prop1.DeclaringType)));
                if (result.Prop2 != null)
                {
                    path.Add((ICrmRelationship)result.Prop2.GetValue(Activator.CreateInstance(result.Prop2.DeclaringType)));
                }
                return path;
            }
            if (via == null)
            {
                PropertyInfo relationshipTypeInfo = null;

                var allRelationships = from property in source.GetProperties()
                                       where property.PropertyType.IsGenericType &&
                                       property.PropertyType.GenericTypeArguments.Contains(target) &&
                                       property.PropertyType.GetInterfaces().Contains(typeof(ICrmRelationship))
                                       select property;


                if (!string.IsNullOrEmpty(sourcePropertyName))
                {
                    foreach (var property in allRelationships)
                    {
                        if (string.Equals(property.Name, sourcePropertyName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            relationshipTypeInfo = property;
                            break;
                        }
                        var oDataAttribute = property.GetCustomAttribute<ODataQueryAttribute>();
                        if (oDataAttribute != null &&
                            oDataAttribute.QueryName == sourcePropertyName)
                        {
                            relationshipTypeInfo = property;
                            break;
                        }
                    }
                }
                else
                {
                    relationshipTypeInfo = allRelationships.FirstOrDefault();
                }

                _relationshipPaths.AddOrUpdate((source, target, via, sourcePropertyName), (relationshipTypeInfo, null), (k, r) => (relationshipTypeInfo, null));

                if (relationshipTypeInfo != null)
                {
                    var rel = (ICrmRelationship)relationshipTypeInfo.GetValue(Activator.CreateInstance(relationshipTypeInfo.DeclaringType));
                    path.Add(rel);
                    return path;
                }
            }

            //Project, Organization (via Account)
            IEnumerable<Type[]> sourceRelationships;
            IEnumerable<Type[]> targetRelationships;
            if (via != null)
            {
                sourceRelationships = from property in source.GetProperties()
                                      where property.PropertyType.IsGenericType &&
                                      property.PropertyType.GetInterfaces().Contains(typeof(ICrmRelationship)) &&
                                      property.PropertyType.GenericTypeArguments.Contains(via)
                                      select property.PropertyType.GenericTypeArguments;

                targetRelationships = from property in target.GetProperties()
                                      where property.PropertyType.IsGenericType &&
                                      property.PropertyType.GetInterfaces().Contains(typeof(ICrmRelationship)) &&
                                      property.PropertyType.GenericTypeArguments.Contains(via)
                                      select property.PropertyType.GenericTypeArguments;
            }
            else
            {
                //Alle Relationen von Projekt (hier benötigen wir Project.ProjectRole)
                sourceRelationships = from property in source.GetProperties()
                                      where property.PropertyType.IsGenericType &&
                                      property.PropertyType.GetInterfaces().Contains(typeof(ICrmRelationship))
                                      select property.PropertyType.GenericTypeArguments;

                //Alle Relation von Contact (hier benötigen wir Contact.ProjectRoles)
                targetRelationships = from property in target.GetProperties()
                                      where property.PropertyType.IsGenericType &&
                                      property.PropertyType.GetInterfaces().Contains(typeof(ICrmRelationship))
                                      select property.PropertyType.GenericTypeArguments;
            }

            //ProjectProjectRoles
            //ContactProjectRoles

            sourceRelationships = sourceRelationships.Where(i => i.Last() != target);

            var relationships = from rel1 in sourceRelationships
                                from rel2 in targetRelationships
                                where rel1.Last() == rel2.Last()
                                select rel1.Last();


            if (!relationships.Any())
            {
                _relationshipPaths.AddOrUpdate((source, target, via, sourcePropertyName), (null, null), (k, r) => (null, null));
                return null;
            }

            foreach (var joinType in relationships)
            {
                var join1 = (from property in source.GetProperties()
                             where property.PropertyType.GetInterfaces().Contains(typeof(ICrmRelationship)) &&
                                   property.PropertyType.GenericTypeArguments.Contains(joinType)
                             select property).FirstOrDefault(); //ProjectAccount (zu "Link-Tabelle")

                var join2 = (from property in joinType.GetProperties()
                             where property.PropertyType.GetInterfaces().Contains(typeof(ICrmRelationship)) &&
                                  property.PropertyType.GenericTypeArguments.Contains(target)
                             select property).FirstOrDefault();//AccountOrganization

                if (join1 == null)
                {
                    continue;
                }

                if (join2 == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(sourcePropertyName) && !string.Equals(join2.Name, sourcePropertyName, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                path.Add((ICrmRelationship)join1.GetValue(Activator.CreateInstance(join1.DeclaringType)));
                path.Add((ICrmRelationship)join2.GetValue(Activator.CreateInstance(join2.DeclaringType)));

                _relationshipPaths.AddOrUpdate((source, target, via, sourcePropertyName), (join1, join2), (k, r) => (join1, join2));

                return path;
            }

            return null;
        }

        #endregion

        #region GetRelationshipNames

        public static IEnumerable<string> GetFirstRelationshipNames(Type source, Type target, Type via = null, string sourcePropertyName = null)
        {
            var names = new List<string>();
            if (_relationshipPathNames.TryGetValue((source, target, via, sourcePropertyName), value: out (string Prop1, string Prop2) result))
            {
                if (result.Prop1 == null)
                {
                    return null;
                }

                names.Add(result.Prop1);
                if (result.Prop2 != null)
                {
                    names.Add(result.Prop2);
                }
                return names;
            }
            var relationships = GetRelationshipPath(source, target, via, sourcePropertyName);
            if (relationships != null && relationships.Any())
            {
                string name1 = relationships.First().RelationshipLogicalName;
                names.Add(name1);
                string name2 = null;
                if (relationships.Count() > 1)
                {
                    name2 = relationships.ElementAt(1).RelationshipLogicalName;
                    names.Add(name2);
                }
                _relationshipPathNames.AddOrUpdate((source, target, via, sourcePropertyName), (name1, name2), (k, v) => (name1, name2));
            }
            else
            {
                _relationshipPathNames.AddOrUpdate((source, target, via, sourcePropertyName), (null, null), (k, v) => (null, null));
            }

            return names;
        }

        #endregion

        #endregion
    }
}
