namespace System.Reflection
{
    public static class TypeUtility
    {
        #region GetPrivateFieldValue

        public static T GetPrivateFieldValue<T>(this Type type, object instance, string fieldName)
        {
            var fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            var value = fieldInfo.GetValue(instance);

            if (value is T) return (T)value;
            return default(T);
        }

        #endregion

    }
}
