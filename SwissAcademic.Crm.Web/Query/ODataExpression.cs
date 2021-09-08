using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace SwissAcademic.Crm.Web
{
    public class ODataExpression
        :
        ExpressionBase
    {
        #region Konstruktor

        public ODataExpression(string odata)
        {
            if (string.IsNullOrEmpty(odata))
            {
                throw new NotSupportedException("OData-String must not be null or empty.");
            }
            OData = odata.Substring(odata.IndexOf("?", StringComparison.InvariantCultureIgnoreCase) + 1);

            EntityName = EntityNameResolver.GetSignlarTypeName(odata.Substring(0, odata.IndexOf("?", StringComparison.InvariantCultureIgnoreCase)));
        }

        #endregion

        #region Eigenschaften

        public override string EntityName { get; }

        internal string OData { get; }

        #endregion

        #region Methoden

        public override string ToOData()
        {
            if (!string.IsNullOrEmpty(NextLink))
            {
                return NextLink;
            }
            return OData;
        }

        #endregion
    }
}
