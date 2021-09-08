using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace Newtonsoft.Json
{
    public abstract class CustomDataMemberAttribute
        :
        Attribute
    {
        #region Constructors

        public CustomDataMemberAttribute()
        {
        }

        #endregion

        #region Properties

        #region Name

        public virtual string Name { get; set; }

        #endregion

        #endregion

        #region Methods

        #region ShouldSerialize

        public virtual bool ShouldSerialize(MemberInfo member, JsonProperty jsonProperty, object instance) => true;

        #endregion

        #endregion
    }
}
