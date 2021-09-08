using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SwissAcademic.Collections
{
    public class PropertyComparer<T>
        :
        IComparer<T>
    {
        #region Konstruktoren

        public PropertyComparer(IEnumerable<PropertySortDescriptor<T>> sortDescriptors)
        {
            if (sortDescriptors == null) throw new ArgumentNullException();
            SortDescriptors = sortDescriptors;
        }

        public PropertyComparer(params PropertySortDescriptor<T>[] sortDescriptors)
        {
            if (sortDescriptors == null) throw new ArgumentNullException();
            SortDescriptors = sortDescriptors;
        }

        #endregion

        #region Eigenschaften

        #region SortDescriptors

        IEnumerable<PropertySortDescriptor<T>> _sortDescriptors;

        public IEnumerable<PropertySortDescriptor<T>> SortDescriptors
        {
            get { return _sortDescriptors; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                _sortDescriptors = value;
            }
        }

        #endregion

        #endregion

        #region Methoden

        #region Compare

        public int Compare(T x, T y)
        {
            int result = 0;
            int directionFactor;

            foreach (var sortDescriptor in SortDescriptors)
            {
                directionFactor = sortDescriptor.SortDirection == ListSortDirection.Ascending ? 1 : -1;
                result = sortDescriptor.PropertyDescriptor.Compare(x, y) * directionFactor;

                if (result != 0) break;
            }

            return result;
        }

        #endregion

        #endregion
    }
}
