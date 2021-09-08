using System.Collections.Generic;

namespace System.Linq
{
    public static class LinqUtility
    {
        #region Add

        //ME: Ich hab das ausgeklammert. Das führt zu "Fehlern" im Code.
        //Da wir kein Add durchgefügrt oder so
        //public static IEnumerable<T> Add<T>(this IEnumerable<T> existingItems, T newItem)
        //{
        //	foreach (var item in existingItems)
        //	{
        //		yield return item;
        //	}
        //	yield return newItem;		
        //}

        #endregion Add

        #region IndexOf

        ///<summary>Finds the index of the first occurence of an item in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="item">The item to find.</param>
        ///<returns>The index of the first matching item, or -1 if the item was not found.</returns>
        public static int IndexOf<T>(this IEnumerable<T> items, T item)
        {
            return items.FindFirstIndex(i => EqualityComparer<T>.Default.Equals(item, i));
        }

        #endregion IndexOf

        #region FindFirstIndex

        ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="predicate">The expression to test the items against.</param>
        ///<returns>The index of the first matching item, or -1 if no items match.</returns>
        public static int FindFirstIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            int retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }

        #endregion

        #region FindIndex

        ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="predicate">The expression to test the items against.</param>
        ///<returns>The index of the first matching item, or -1 if no items match.</returns>
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            int retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }

        #endregion FindIndex

        #region FindLastIndex

        ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="predicate">The expression to test the items against.</param>
        ///<returns>The index of the first matching item, or -1 if no items match.</returns>
        public static int FindLastIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            if (!items.Any()) return -1;

            int highestIndex = items.Count() - 1;
            int retVal = highestIndex;


            for (var i = highestIndex; i >= 0; i--)
            {
                var item = items.ElementAt(i);
                if (predicate(item)) return retVal;
                retVal--;
            }
            return -1;
        }


        #endregion

        #region FindWithPreviousAndNext

        public static IEnumerable<T> FindWithPreviousAndNext<T>(this IEnumerable<T> items, Predicate<T> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            return FindWithPreviousAndNextImplementation(items, predicate);
        }

        private static IEnumerable<T> FindWithPreviousAndNextImplementation<T>(IEnumerable<T> items, Predicate<T> predicate)
        {
            using (var iter = items.GetEnumerator())
            {
                T previous = default(T);
                while (iter.MoveNext())
                {
                    if (predicate(iter.Current))
                    {
                        yield return previous;
                        yield return iter.Current;
                        if (iter.MoveNext())
                            yield return iter.Current;
                        else
                            yield return default(T);
                        yield break;
                    }
                    previous = iter.Current;
                }
            }
            // If we get here nothing has been found so return three default values
            yield return default(T); // Previous
            yield return default(T); // Current
            yield return default(T); // Next
        }

        #endregion FindWithPreviousAndNext

        #region RemoveFromCount

        public static void RemoveFromCount<T>(this List<T> list, int countAfterRemoving)
        {
            var removeCount = list.Count - countAfterRemoving;

            if (removeCount <= 0) return;

            list.RemoveRange(0, countAfterRemoving);
        }

        #endregion

        #region RemoveLastFromCount

        public static List<T> RemoveLastFromCount<T>(this List<T> list, int countAfterRemoving)
        {
            var results = new List<T>();
            while (true)
            {
                if (list.Count <= countAfterRemoving) break;

                var l = list.Last();

                results.Add(l);
                list.Remove(l);
            }

            return results;
        }

        #endregion

        #region SequenceEqual

        public static bool SequenceEqual<T, TKey>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, TKey> keyExtractor, IEqualityComparer<TKey> keyEqualityComparer)
        {
            var equalityComparer = new KeyEqualityComparer<T, TKey>(keyExtractor, keyEqualityComparer);
            return first.SequenceEqual(second, equalityComparer);
        }

        #endregion

        #region Distinct

        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> collection, Func<T, TKey> keyExtractor, IEqualityComparer<TKey> keyEqualityComparer)
        {
            var equalityComparer = new KeyEqualityComparer<T, TKey>(keyExtractor, keyEqualityComparer);
            return collection.Distinct(equalityComparer);
        }

        #endregion
    }

    public class KeyEqualityComparer<T, TKey>
        :
        IEqualityComparer<T>
    {
        #region Felder

        Func<T, TKey> _keyExtractor;
        IEqualityComparer<TKey> _keyEqualityComparer;

        #endregion

        #region Konstruktoren

        public KeyEqualityComparer(Func<T, TKey> keyExtractor, IEqualityComparer<TKey> keyEqualityComparer)
        {
            _keyExtractor = keyExtractor;
            _keyEqualityComparer = keyEqualityComparer;
        }

        #endregion

        #region IEqualityComparer<T> Members

        public bool Equals(T x, T y)
        {
            if (_keyEqualityComparer == null) return _keyExtractor(x).Equals(_keyExtractor(y));
            return _keyEqualityComparer.Equals(_keyExtractor(x), _keyExtractor(y));
        }

        public int GetHashCode(T obj)
        {
            return _keyEqualityComparer.GetHashCode(_keyExtractor(obj));
        }

        #endregion
    }
}
