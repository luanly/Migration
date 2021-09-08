using System;
namespace SwissAcademic.Collections
{
    public interface IPropertyDescriptor
    {
        Type ComponentType { get; }
        Enum PropertyEnum { get; }
        object GetValue(object item);
        void SetValue(object item, object value);
    }
}
