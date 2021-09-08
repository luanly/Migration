using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json
{
    #region StringValueProvider

    internal class StringValueProvider
        :
        IValueProvider
    {
        #region Fields

        readonly IValueProvider _baseValueProvider;

        #endregion

        #region Constructors

        public StringValueProvider(IValueProvider baseValueProvider)
        {
            _baseValueProvider = baseValueProvider;
        }

        #endregion

        #region Methods

        #region GetValue

        public object GetValue(object target)
        {
            var value = (string)_baseValueProvider.GetValue(target);
            return (string.IsNullOrEmpty(value)) ? null : value;
        }

        #endregion

        #region SetValue

        public void SetValue(object target, object value)
        {
            _baseValueProvider.SetValue(target, value);
        }

        #endregion

        #endregion
    }

    #endregion
}
