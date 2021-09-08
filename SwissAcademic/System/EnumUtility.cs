using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System
{
    public static class EnumUtility
    {
        #region SetEnumValue

        /// <summary>
        /// Calls the setter action with a converted enum from either a string, int or typed value representation of the enum.
        /// Calls the setter with default(T) if value is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="setter"></param>
        public static void SetEnumValue<T>(object value, Action<T> setter, T valueIfNull = default(T))
        {
            if (value == null || value == DBNull.Value)
            {
                setter(valueIfNull);
            }

            else if (value is string)
            {
                int intValue;
                if (int.TryParse(value.ToString(), out intValue))
                {
                    setter((T)Enum.ToObject(typeof(T), intValue));
                }
                else
                {
                    setter(value.ToString().ParseEnum<T>());
                }
            }

            else
            {
                setter((T)Enum.ToObject(typeof(T), value));
            }
        }

        #endregion

        #region SortEnum

        public static IEnumerable<TEnum> EnumGetOrderedValues<TEnum>(this Type enumType)
        {
            if (!enumType.IsEnum) throw new ArgumentException("TEnum must be an enumerated type");

            var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            var orderedValues = new List<Tuple<int, TEnum>>();
            foreach (var field in fields)
            {
                var orderAtt = field.GetCustomAttributes(typeof(OrderAttribute), false).SingleOrDefault() as OrderAttribute;
                if (orderAtt != null)
                {
                    orderedValues.Add(new Tuple<int, TEnum>(orderAtt.Order, (TEnum)field.GetValue(null)));
                }
                else
                {
                    orderedValues.Add(new Tuple<int, TEnum>(int.MaxValue, (TEnum)field.GetValue(null)));
                }
            }

            return orderedValues.OrderBy(x => x.Item1).Select(x => x.Item2).ToList();
        }

        #endregion

        #region WillUpdate

        public static bool WillUpdate<T>(object value, Func<T> getter, T valueIfNull = default(T))
        {
            T enumValue;

            if (value == null || value == DBNull.Value)
            {
                enumValue = valueIfNull;
            }

            else if (value is string)
            {
                int intValue;
                if (int.TryParse(value.ToString(), out intValue))
                {
                    enumValue = (T)Enum.ToObject(typeof(T), intValue);
                }
                else
                {
                    enumValue = value.ToString().ParseEnum<T>();
                }
            }

            else
            {
                enumValue = (T)Enum.ToObject(typeof(T), value);
            }

            return !getter().Equals(enumValue);
        }

        #endregion
    }

    #region Class OrderAttribute

    public class OrderAttribute
        :
        Attribute
    {
        public readonly int Order;

        public OrderAttribute(int order)
        {
            Order = order;
        }
    }

    #endregion
}