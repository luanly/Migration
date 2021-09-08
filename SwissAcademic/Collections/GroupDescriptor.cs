using System;

namespace SwissAcademic.Collections
{
    [Serializable]
    public class GroupDescriptor<T>
    {
        #region Konstruktoren

        public GroupDescriptor(PropertyDescriptor<T> property)
            :
            this(property, null)
        {
        }

        public GroupDescriptor(PropertyDescriptor<T> property, Comparison<object> sortComparison)
        {
            Property = property;
            SortComparison = sortComparison;
        }

        #endregion

        #region Eigenschaften

        #region PropertyDescriptor

        public PropertyDescriptor<T> Property { get; private set; }

        #endregion

        #region SortComparison

        public Comparison<object> SortComparison { get; set; }

        #endregion

        #endregion
    }
}
