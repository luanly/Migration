using System;
using System.ComponentModel;
using System.Globalization;

namespace SwissAcademic.Collections
{
    [Serializable]
    public class PropertySortDescriptor<T>
        :
        IEquatable<PropertySortDescriptor<T>>
    {
        #region Konstruktoren

        public PropertySortDescriptor(PropertyDescriptor<T> propertyDescriptor, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            PropertyDescriptor = propertyDescriptor;
            SortDirection = sortDirection;
        }

        #endregion

        #region Eigenschaften

        #region PropertyDescriptor

        PropertyDescriptor<T> _propertyDescriptor;

        public PropertyDescriptor<T> PropertyDescriptor
        {
            get { return _propertyDescriptor; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                _propertyDescriptor = value;
            }
        }

        #endregion

        #region SortDirection

        public ListSortDirection SortDirection { get; set; }

        #endregion

        #endregion

        #region Methoden

        #region ToString

        public override string ToString()
        {
            return string.Format("{0}, {1}", PropertyDescriptor.GetNameLocalized(false), SwissAcademic.Resources.ResourceHelper.GetEnumName(SortDirection));
        }

        public string ToString(CultureInfo culture)
        {
            return string.Format("{0}, {1}", PropertyDescriptor.GetNameLocalized(false, culture), SwissAcademic.Resources.ResourceHelper.GetEnumName(SortDirection, culture));
        }

        #endregion

        #endregion

        #region IEquatable

        #region Equals

        public override bool Equals(object obj)
        {
            return Equals(obj as PropertySortDescriptor<T>);
        }

        public bool Equals(PropertySortDescriptor<T> other)
        {
            if (other == null) return false;

            if (PropertyDescriptor == null || other.PropertyDescriptor == null) return false;
            if (PropertyDescriptor.GetType() != other.PropertyDescriptor.GetType()) return false;

            //SortDirection is of enum type ListSortDirection and never null, but by default ListSortDirection.Ascending
            return PropertyDescriptor.Equals(other.PropertyDescriptor) && SortDirection.Equals(other.SortDirection);
        }

        #endregion

        #region GetHashCode

        public override int GetHashCode()
        {
            return PropertyDescriptor.GetHashCode() ^ SortDirection.GetHashCode();
        }

        #endregion //GetHashCode

        #endregion //IEquatable
    }
}
