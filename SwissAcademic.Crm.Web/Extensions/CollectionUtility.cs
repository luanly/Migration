using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	public static class CrmCollectionUtility
	{
        public static void AddIfCrmEntityNotExists<T>(this List<T> list, T item)
            where T: CitaviCrmEntity
        {
            if(list.Any(i => i.Id == item.Id))
			{
                return;
			}
            if (list.Any(i => i.Key == item.Key))
            {
                return;
            }
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }
    }
}
