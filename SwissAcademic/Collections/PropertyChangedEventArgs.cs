using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace SwissAcademic.Collections
{
    [DataContract]
    public class PropertyChangedEventArgs<T>
        :
        EventArgs,
        IPropertyChangedEventArgs
    {
        #region Konstruktoren

        public PropertyChangedEventArgs(PropertyDescriptor<T> property, object oldValue)
            :
            this(property, oldValue, null, null)
        { }

        [JsonConstructor]
        public PropertyChangedEventArgs(PropertyDescriptor<T> property, object oldValue, ICollectionChangedEventArgs collectionChangedTrigger, IPropertyChangedEventArgs propertyChangedTrigger)
        {
            Property = property;
            CollectionChangedTrigger = collectionChangedTrigger;
            _oldValue = oldValue;
            PropertyChangedTrigger = propertyChangedTrigger;
        }

        public PropertyChangedEventArgs(bool isReset)
        {
            IsReset = isReset;
        }

        #endregion

        #region Eigenschaften

        #region CollectionChangedTrigger

        [DataMember]
#if !Web

#pragma warning disable CA2326, SCS0028 // Verwenden Sie keinen anderen TypeNameHandling-Wert als "None".
        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
#pragma warning restore CA2326, SCS0028 // Verwenden Sie keinen anderen TypeNameHandling-Wert als "None".

#endif
        public ICollectionChangedEventArgs CollectionChangedTrigger { get; private set; }

        #endregion

        #region IsReset

        [DataMember]
        public bool IsReset { get; private set; }

        #endregion

        #region OldValue

        [NonSerialized]
        object _oldValue;

        public object OldValue
        {
            get { return _oldValue; }
        }

        #endregion

        #region Property

        [DataMember]
#if !Web
#pragma warning disable CA2326, SCS0028 // Verwenden Sie keinen anderen TypeNameHandling-Wert als "None".
        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
#pragma warning restore CA2326, SCS0028 // Verwenden Sie keinen anderen TypeNameHandling-Wert als "None".
#endif
        public PropertyDescriptor<T> Property { get; private set; }

        #endregion

        #region PropertyChangedTrigger

        [DataMember]
#if !Web
#pragma warning disable CA2326, SCS0028 // Verwenden Sie keinen anderen TypeNameHandling-Wert als "None".
        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
#pragma warning restore CA2326, SCS0028 // Verwenden Sie keinen anderen TypeNameHandling-Wert als "None".
#endif
        public IPropertyChangedEventArgs PropertyChangedTrigger { get; private set; }

        #endregion

        #endregion

        #region Methoden

        #region ToString

        public override string ToString()
        {
            return string.Format("IsReset: {0}\r\nProperty: {1}\r\nOldValue: {2}\r\nCollectionChangedTrigger: {3}\r\nPropertyChangedTrigger: {4}",
                IsReset,
                Property == null ? "null" : Property.ToString(),
                OldValue == null ? "null" : OldValue,
                CollectionChangedTrigger == null ? "null" : CollectionChangedTrigger.ToString(),
                PropertyChangedTrigger == null ? "null" : PropertyChangedTrigger.ToString());
        }

        #endregion

        #endregion

        #region IPropertyChangedEventArgs Members

        IPropertyDescriptor IPropertyChangedEventArgs.Property
        {
            get { return Property; }
        }

        #endregion
    }
}
