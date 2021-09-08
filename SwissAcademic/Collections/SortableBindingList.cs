using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SwissAcademic.Collections
{
    /// <summary>
    /// Provides a generic collection that supports data binding and additionally supports sorting.
    /// See http://msdn.microsoft.com/en-us/library/ms993236.aspx
    /// If the elements are IComparable it uses that; otherwise compares the ToString()
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public abstract class SortableBindingList<T>
        :
        BindingList<T>
    {
        #region Konstruktoren

        public SortableBindingList(List<T> innerList)
        {
            ((List<T>)Items).AddRange(innerList);
        }

        public SortableBindingList()
        {
        }

        #endregion //Konstruktoren

        #region Felder

        private bool _isSorted;
        private ListSortDirection _sortDirection;
        private PropertyDescriptor _sortProperty;

        #endregion //Felder

        #region Eigenschaften

        /// <summary>
        /// Gets a value indicating whether the list supports sorting.
        /// </summary>
        protected override bool SupportsSortingCore
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the list is sorted.
        /// </summary>
        protected override bool IsSortedCore
        {
            get { return _isSorted; }
        }

        /// <summary>
        /// Gets the direction the list is sorted.
        /// </summary>
        protected override ListSortDirection SortDirectionCore
        {
            get { return _sortDirection; }
        }

        /// <summary>
        /// Gets the property descriptor that is used for sorting the list if sorting is implemented in a derived class; otherwise, returns null
        /// </summary>
        protected override PropertyDescriptor SortPropertyCore
        {
            get { return _sortProperty; }
        }

        #endregion //Eigenschaften

        #region Methoden

        protected abstract Comparison<T> GetComparer(PropertyDescriptor prop);

        /// <summary>
        /// Removes any sort applied with ApplySortCore if sorting is implemented
        /// </summary>
        protected override void RemoveSortCore()
        {
            _sortDirection = ListSortDirection.Ascending;
            _sortProperty = null;
        }

        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
        {
            //for sorting by any member that implements IComparable
            List<T> itemsList = (List<T>)Items;
            if (property.PropertyType.GetInterface("IComparable") != null)
            {
                itemsList.Sort(new Comparison<T>(delegate (T x, T y)
                {
                    // Compare x to y if x is not null. If x is, but y isn't, we compare y
                    // to x and reverse the result. If both are null, they're equal.
                    if (property.GetValue(x) != null)
                        return ((IComparable)property.GetValue(x)).CompareTo(property.GetValue(y)) * (direction == ListSortDirection.Descending ? -1 : 1);
                    else if (property.GetValue(y) != null)
                        return ((IComparable)property.GetValue(y)).CompareTo(property.GetValue(x)) * (direction == ListSortDirection.Descending ? 1 : -1);
                    else
                        return 0;
                }));
            }

            _isSorted = true;
            _sortProperty = property;
            _sortDirection = direction;

            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        #endregion //Methoden
    }
}
