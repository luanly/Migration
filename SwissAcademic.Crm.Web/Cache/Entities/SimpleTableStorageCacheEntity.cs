using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwissAcademic.Crm.Web
{

    public class SimpleTableStorageCacheEntity
        :
        TableEntity
    {
        static readonly Type StringType = typeof(string);

        public readonly static List<string> ColumnNames = new List<string> {
            nameof(P0)
        };

        public string P0 { get; set; }

        #region Methoden

        public T GetData<T>()
        {
            if(typeof(T) == StringType)
			{
                return (T)Convert.ChangeType(P0, StringType);
            }
            return JsonConvert.DeserializeObject<T>(P0);
        }

        public void SetData(object val)
        {
            P0 = val.ToString();
        }


        #endregion
    }
}
