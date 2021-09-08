using System;
using System.Globalization;

namespace SwissAcademic.Collections
{
    public abstract class PropertyDescriptor<T>
        :
        IPropertyDescriptor,
        IEquatable<PropertyDescriptor<T>>
    {
        #region Konstruktoren

        protected PropertyDescriptor()
        {
        }

        #endregion

        #region Eigenschaften

        #region CanSetValue

        public abstract bool CanSetValue { get; }

        #endregion

        #region ComponentType

        public abstract Type ComponentType { get; }

        #endregion

        #region PropertyEnum

        public abstract Enum PropertyEnum { get; }

        #endregion

        #endregion

        #region Methoden

        #region Compare

        public abstract int Compare(T x, T y);

        #endregion

        #region Equals

        public override bool Equals(object obj)
        {
            return Equals(obj as PropertyDescriptor<T>);
        }

        public bool Equals(PropertyDescriptor<T> other)
        {
            if (other == null) return false;
            //JHP nachfolgende Zeile ist m.E. zu streng, denn sie prüft auf System.Object.ReferenceEquals
            //return GetPropertyIdAsEnum() == other.GetPropertyIdAsEnum();
            return PropertyEnum.Equals(other.PropertyEnum);
        }

        #endregion

        #region GetHashCode

        public override int GetHashCode()
        {
            return PropertyEnum.GetHashCode();
        }

        #endregion

        #region GetNameLocalized

        public abstract string GetNameLocalized(bool shortVersion = false, CultureInfo culture = null);

        public virtual string GetNameLocalized(T item, bool shortVersion = false)
        {
            return GetNameLocalized(shortVersion);
        }

        #endregion

        #region GetValue

        public abstract object GetValue(T item);

        #endregion

        #region GetValueAsGroupHeader

        public virtual object GetValueAsGroupHeader(T item)
        {
            return GetValue(item);
        }

        #endregion

        #region GetValueAsString

        public virtual string GetValueAsString(T item)
        {
            object objectValue = GetValue(item);

            if (objectValue == null) return string.Empty;

            var enumValue = objectValue as Enum;

            if (enumValue != null)
            {
                return SwissAcademic.Resources.ResourceHelper.GetEnumName(enumValue);
            }

            if (objectValue is DateTime)
            {
                var dt = (DateTime)objectValue;

                if (dt == SwissAcademic.Environment.NullDate)
                {
                    return string.Empty;
                }

                if (dt.TimeOfDay.Ticks == 0)
                {
                    return dt.ToString("D");
                }

                return dt.ToString("f");
            }

            return objectValue.ToString();
        }

        #endregion

        #region SetValue

        public abstract void SetValue(T item, object value);

        #endregion

        #region ToString

        public override string ToString()
        {
            return PropertyEnum.ToString();
        }

        #endregion

        #endregion

        #region IPropertyDescriptor Members

        object IPropertyDescriptor.GetValue(object item)
        {
            return GetValue((T)item);
        }

        void IPropertyDescriptor.SetValue(object item, object value)
        {
            SetValue((T)item, value);
        }

        #endregion
    }
}
