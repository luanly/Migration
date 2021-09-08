using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.SqlClient
{
    public static class SqlCommandExtensions
    {
        public static string QuoteName(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return $"[{value.Replace("]", "]]")}]";
        }
    }
}
