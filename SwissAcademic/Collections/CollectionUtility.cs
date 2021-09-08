using Newtonsoft.Json.Linq;
using SwissAcademic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Collections.Generic
{
    #region CollectionUtility
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>	Provides utility methods for collections. </summary>
    ///
    /// <remarks>	Thomas Schempp, 14.01.2010. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public static class CollectionUtility
    {
        #region Konstanten

        internal const int DuplicatesCheckHashSetThreshold = 200;

        #endregion

        #region AddIfNotExists

        public static void AddIfNotExists<T>(this List<T> list, T item)
        {
            if (!list.Contains(item)) list.Add(item);
        }

        #endregion

        #region AddRange

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> newCollectionItems)
        {
            var list = collection as List<T>;

            if (list != null)
            {
                list.AddRange(newCollectionItems);
            }
            else
            {
                foreach (T item in newCollectionItems)
                {
                    collection.Add(item);
                }
            }
        }

        #endregion

        #region Append

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T item)
        {
            foreach (var i in source)
                yield return i;

            yield return item;
        }

        #endregion Append

        #region Compare

        public static int CompareTo<T>(this IEnumerable<T> collection, IEnumerable<T> other)
        {
            return CompareTo(collection, other, null, false);
        }

        public static int CompareTo<T>(this IEnumerable<T> collection, IEnumerable<T> other, IComparer<T> comparer)
        {
            return CompareTo(collection, other, comparer, false);
        }

        public static int CompareTo<T>(this IEnumerable<T> collection, IEnumerable<T> other, IComparer<T> comparer, bool sortEmptyListLast)
        {
            if (collection == null)
            {
                if (other == null) return 0;
                return sortEmptyListLast ? 1 : -1;
            }

            if (other == null) return sortEmptyListLast ? -1 : 1;

            if (!collection.Any())
            {
                if (!other.Any()) return 0;
                return sortEmptyListLast ? 1 : -1;
            }

            if (!other.Any()) return sortEmptyListLast ? -1 : 1;

            int result = 0;

            var collectionEnumerator = collection.GetEnumerator();
            var otherEnumerator = other.GetEnumerator();

            if (comparer == null)
            {
                if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
                {
                    while (collectionEnumerator.MoveNext())
                    {
                        if (otherEnumerator.MoveNext())
                        {
                            result = ((IComparable<T>)collectionEnumerator.Current).CompareTo(otherEnumerator.Current);
                            if (result != 0) break;
                        }
                        else
                        {
                            // collection has more items than other
                            result = 1;
                            break;
                        }
                    }

                    if (result == 0 && otherEnumerator.MoveNext())
                    {
                        // other has more items than collection
                        result = -1;
                    }
                }
                else
                {
                    result = 0;
                }
            }
            else
            {
                while (collectionEnumerator.MoveNext())
                {
                    if (otherEnumerator.MoveNext())
                    {
                        result = comparer.Compare(collectionEnumerator.Current, otherEnumerator.Current);
                        if (result != 0) break;
                    }
                    else
                    {
                        // collection has more items than other
                        result = 1;
                        break;
                    }
                }

                if (result == 0 && otherEnumerator.MoveNext())
                {
                    // other has more items than collection
                    result = -1;
                }
            }

            return result;
        }

        #endregion

        #region Concat

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> collection, params T[] second)
        {
            if (second == null) return collection;
            return Enumerable.Concat(collection, second);
        }

        #endregion

        //JHP Re-Added because used often in macros and citation style condition scripts
        #region ContentEquals

        public static bool ContentEquals<T>(this IEnumerable<T> first, IEnumerable<T> second, bool considerSortOrder)
        {
            return ContentEquals(first, second, considerSortOrder, null);
        }

        public static bool ContentEquals<T>(this IEnumerable<T> first, IEnumerable<T> second, bool considerSortOrder, IEqualityComparer<T> comparer)
        {
            if (considerSortOrder)
            {
                return first.SequenceEqual(second, comparer);
            }

            var firstList = first.ToList();
            var secondList = second.ToList();

            if (firstList.Count != secondList.Count) return false;
            if (firstList.Count == 0 && secondList.Count == 0) return true;

            while (firstList.Count != 0)
            {
                var index = comparer == null ?
                    secondList.FindIndex(item => item.Equals(firstList[0])) :
                    secondList.FindIndex(item => comparer.Equals(item, firstList[0]));

                if (index == -1) return false;

                firstList.RemoveAt(0);
                secondList.RemoveAt(index);
            }

            return true;
        }

        #endregion ContentEquals

        #region Count

        public static int Count<T>(this IEnumerable<T> collection, Predicate<T> match, int start, int end)
        {
            if (end < start) return 0;

            var count = 0;
            var index = -1;

            foreach (T item in collection)
            {
                index++;
                if (index < start) continue;
                if (index > end) break;
                if (match(item)) count++;
            }

            return count;
        }

        #endregion

        #region EnumerableFrom

        public static IEnumerable<T> EnumerableFrom<T>(T item)
        {
            //return new T[] { item };
            yield return item;
        }

        #endregion EnumerableFrom

        #region Except

        public static IEnumerable<T> Except<T>(this IEnumerable<T> collection, params T[] second)
        {
            if (second == null) return collection;
            return Enumerable.Except(collection, second);
        }

        #endregion

        #region ForEach

        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            foreach (var item in sequence) action(item);
        }

        #endregion

        #region GetValueOrDefault

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (key == null) return default;

            TValue value;
            return dictionary.TryGetValue(key, out value) ?
                value : 
                default;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (key == null) return default;
            
            TValue value;
            return dictionary.TryGetValue(key, out value) ? 
                value : 
                defaultValue == null ? default : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueProvider)
        {
            TValue value;
            if (key == null) return default;

            return dictionary.TryGetValue(key, out value) ? 
                value : 
                defaultValueProvider == null ? default : defaultValueProvider();
        }

        #endregion

        #region Prepend

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T item)
        {
            yield return item;

            foreach (T i in source)
                yield return i;
        }

        #endregion Prepend

        #region PrependIfNotExists

        public static IEnumerable<T> PrependIfNotExists<T>(this List<T> list, T item)
        {
            return !list.Contains(item) ? list.Prepend(item) : list;
        }

        #endregion

        #region RemoveAll

        public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            var items = (from item in collection
                         where predicate(item)
                         select item).ToList();

            foreach (var item in items)
            {
                collection.Remove(item);
            }
        }

        #endregion

        #region Shuffle

        //JHP to simulate random order, e.g. in citation style preview ("in order of appearance")
        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
#pragma warning disable SCS0005, CA5394 // Weak random generator
                int k = rng.Next(n + 1);
#pragma warning restore SCS0005, CA5394 // Weak random generator
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        #endregion Shuffle

        #region ToDictionary

        public static Dictionary<string, string> ToDictionary(params object[] keysAndValues)
        {
            if (keysAndValues == null || !keysAndValues.Any()) return null;

            Dictionary<string, string> properties;

            #region Error handling: keysAndValues has an uneven count

            if (keysAndValues.Length % 2 != 0)
            {
                throw new InvalidOperationException($"{nameof(keysAndValues)} has an uneven count.");
            }

            #endregion

            properties = new Dictionary<string, string>(keysAndValues.Length / 2);

            for (int i = 0; i < keysAndValues.Length; i++)
            {
                string key;
                string value;

                #region Get key[i].ToString()

                try
                {
                    if (keysAndValues[i] == null)
                    {
                        throw new ArgumentException($"Key {i} is null");
                    }
                    else
                    {
                        try
                        {
                            key = keysAndValues[i].ToString();
                        }
                        catch (Exception exception)
                        {
                            throw new ArgumentException($"{nameof(keysAndValues)}[{i}].ToString() threw exception", exception);
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw new ArgumentException($"Accessing {nameof(keysAndValues)}[{i}] threw exception", exception);
                }

                #endregion

                #region Get value[j].ToString()

                var j = i + 1;
                try
                {
                    if (keysAndValues[j] == null)
                    {
                        value = null;
                    }
                    else
                    {
                        try
                        {
                            value = keysAndValues[j].ToString();
                        }
                        catch (Exception exception)
                        {
                            throw new ArgumentException($"{nameof(keysAndValues)}[{j}].ToString() threw exception", exception);
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw new ArgumentException($"Accessing {nameof(keysAndValues)}[{j}] threw exception", exception);
                }

                #endregion

                properties[key] = value;
                i++;
            }

            return properties;
        }

        #endregion

        #region GetConnectionId

        public static string GetConnectionId(this IDictionary<string, string> keyValuePairs)
        {
            if(!keyValuePairs.TryGetValue(MessageKey.ConnectionId, out var connectionId))
            {
                keyValuePairs.TryGetValue(MessageKey.SignalRConnectionId, out connectionId);
            }
            return connectionId;
        }

        public static string GetConnectionId(this JObject jObject)
        {
            if (!jObject.TryGetValue(MessageKey.ConnectionId, out var connectionId))
            {
                jObject.TryGetValue(MessageKey.SignalRConnectionId, out connectionId);
            }
            return connectionId?.Value<string>();
        }

        #endregion

        #region ToString

        public static string ToString(this Enum value, string separator)
        {
            var flagValues = from item in Enum.GetValues(value.GetType()).Cast<Enum>()
                             where value.HasFlag(item)
                             select item;

            return flagValues.ToString(separator);
        }

        public static string ToString<T>(this IEnumerable<T> values, string separator)
        {
            return string.Join(separator, values);
        }

        #endregion

        #region TryGetValue

        //JHP wird das noch gebraucht?
        public static string TryGetStringValue<TKey, TValue>(this IDictionary<TKey, TValue> properties, TKey key)
        {
            if (properties == null) return null;

            TValue value;
            if (properties.TryGetValue(key, out value)) return value.ToString();
            return null;
        }

        #endregion
    }

    #endregion


}

namespace SwissAcademic.Collections
{
    public static class CollectionGroupUtility
    {
        #region AddGroup

        static void AddGroup<T>(IList<Group<T>> groups, GroupDescriptor<T> groupDescriptor, object header, T item)
        {
            var group = header == null ?
                groups.FirstOrDefault(groupItem => groupItem.Header == null) :
                groups.FirstOrDefault(groupItem => header.Equals(groupItem.Header));

            if (group == null)
            {
                group = new Group<T>(groupDescriptor, header);
                group.Items.Add(item);
                groups.Add(group);
            }
            else
            {
                group.Items.Add(item);
            }
        }

        #endregion

        #region GetLowestIndex / GetHighestIndex

        public static int GetLowestIndex<T>(this IList<T> collection, IList<T> items)
        {
            var lowestIndex = collection.Count;

            foreach (var item in items)
            {
                var index = collection.IndexOf(item);
                if (index == -1) continue;
                lowestIndex = Math.Min(lowestIndex, index);
            }

            return lowestIndex;
        }

        public static int GetHighestIndex<T>(this IList<T> collection, IList<T> items)
        {
            var highestIndex = -1;

            foreach (var item in items)
            {
                var index = collection.IndexOf(item);
                if (index == -1) continue;
                highestIndex = Math.Max(highestIndex, index);
            }

            return highestIndex;
        }

        #endregion

        #region Group

        public static IEnumerable<Group<T>> Group<T>(this IEnumerable<T> collection, GroupDescriptor<T> groupDescriptor)
        {
            return Group(collection, new List<GroupDescriptor<T>>() { groupDescriptor });
        }

        public static IEnumerable<Group<T>> Group<T>(this IEnumerable<T> collection, IEnumerable<GroupDescriptor<T>> groupDescriptors)
        {
            if (groupDescriptors == null || !groupDescriptors.Any()) return null;
            if (collection == null || !collection.Any()) return null;

            return Group(collection, groupDescriptors, 0);
        }

        static IEnumerable<Group<T>> Group<T>(this IEnumerable<T> collection, IEnumerable<GroupDescriptor<T>> groupDescriptors, int index)
        {
            var groupDescriptor = groupDescriptors.ElementAt(index);
            var groups = new List<Group<T>>();

            foreach (var item in collection)
            {
                var header = groupDescriptor.Property.GetValueAsGroupHeader(item);
                var headerCollection = header as System.Collections.ICollection;

                if (headerCollection == null)
                {
                    AddGroup(groups, groupDescriptor, header, item);
                }
                else
                {
                    if (headerCollection.Count == 0)
                    {
                        AddGroup(groups, groupDescriptor, null, item);
                    }
                    else
                    {
                        foreach (var headerCollectionItem in headerCollection)
                        {
                            AddGroup(groups, groupDescriptor, headerCollectionItem, item);
                        }
                    }
                }
            }

            SortGroups<T>(groupDescriptor, groups);

            if (index < groupDescriptors.Count() - 1)
            {
                foreach (var group in groups)
                {
                    group.Subgroups = Group(group.Items, groupDescriptors, index + 1);
                    foreach (var subgroup in group.Subgroups) subgroup.Parent = group;
                    group.Items.Clear();
                }
            }

            return groups;
        }

        #endregion

        #region SortGroup

        static void SortGroups<T>(GroupDescriptor<T> groupDescriptor, List<Group<T>> groups)
        {
            if (groups.Count < 2) return;

            if (groupDescriptor.SortComparison == null)
            {
                if (groups[0].Header is IComparable)
                {
                    groups.Sort((x, y) =>
                    {
                        if (x.Header == null)
                        {
                            if (y.Header == null) return 0;
                            return -1;
                        }

                        if (y.Header == null) return 1;

                        return ((IComparable)x.Header).CompareTo(y.Header);
                    });
                }
                else
                {
                    groups.Sort((x, y) =>
                    {
                        if (x.Header == null)
                        {
                            if (y.Header == null) return 0;
                            return -1;
                        }

                        if (y.Header == null) return 1;

                        return x.Header.ToString().CompareTo(y.Header.ToString());
                    });
                }
            }

            else
            {
                groups.Sort((x, y) => groupDescriptor.SortComparison(x.Header, y.Header));
            }
        }

        #endregion

        #region TraverseHierarchy

        public static IEnumerable<T> TraverseHierarchy<T>(this T root, Func<T, bool> hasChildrenSelector, Func<T, IEnumerable<T>> childrenSelector)
        {
            yield return root;

            if (hasChildrenSelector(root))
            {
                foreach (var item in TraverseHierarchy(childrenSelector(root), hasChildrenSelector, childrenSelector))
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> TraverseHierarchy<T>(this IEnumerable<T> collection, Func<T, bool> hasChildrenSelector, Func<T, IEnumerable<T>> childrenSelector)
        {
            foreach (var item in collection)
            {
                yield return item;

                if (hasChildrenSelector(item))
                {
                    foreach (var child in TraverseHierarchy(childrenSelector(item), hasChildrenSelector, childrenSelector))
                    {
                        yield return child;
                    }
                }
            }
        }

        #endregion

        #region FindParentInHierarchy

        public static T FindParentInHierarchy<T>(this T root, T item, Func<T, bool> hasChildrenSelector, Func<T, IEnumerable<T>> childrenSelector)
            where T : class
        {
            if (!hasChildrenSelector(root)) return default(T);
            if (childrenSelector(root).Contains(item)) return root;

            foreach (var child in childrenSelector(root))
            {
                if (child == item) continue;
                if (hasChildrenSelector(child))
                {
                    var childResult = child.FindParentInHierarchy(item, hasChildrenSelector, childrenSelector);
                    if (childResult != null) return childResult;
                }
            }

            return null;
        }

        public static T FindParentInHierarchy<T>(this IEnumerable<T> rootCollection, T item, Func<T, bool> hasChildrenSelector, Func<T, IEnumerable<T>> childrenSelector)
            where T : class
        {
            return (from r in rootCollection
                    let p = r.FindParentInHierarchy(item, hasChildrenSelector, childrenSelector)
                    where p != default(T)
                    select p).FirstOrDefault();
        }

        #endregion

        #region FindParentCollectionInHierarchy

        public static IEnumerable<T> FindParentCollectionInHierarchy<T>(this IEnumerable<T> collection, T item, Func<T, bool> hasChildrenSelector, Func<T, IEnumerable<T>> childrenSelector)
            where T : class
        {
            if (collection.Contains(item)) return collection;

            foreach (var child in collection)
            {
                if (hasChildrenSelector(child))
                {
                    var childResult = childrenSelector(child).FindParentCollectionInHierarchy(item, hasChildrenSelector, childrenSelector);
                    if (childResult != null) return childResult;
                }
            }

            return null;
        }

        #endregion
    }
}

namespace System.Collections.Concurrent
{
    public static class ConcurrentCollectionUtility
    {
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> concurrentDictionary)
        {
            var result = new Dictionary<TKey, TValue>();
            concurrentDictionary.ForEach(i => result.Add(i.Key, i.Value));

            return result;
        }

        public static void Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> concurrentDictionary, TKey key) => concurrentDictionary.TryRemove(key, out _);

        public static void Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> concurrentDictionary, TKey key, TValue value) => concurrentDictionary.TryAdd(key, value);
    }
}
