using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwissAcademic.Crm.Web
{
    public class QueryExpression
        :
        ExpressionBase
    {
        #region Konstruktor

        public QueryExpression(string entityName)
        {
            EntityName = entityName;
        }

        #endregion

        #region Eigenschaften

        public List<ConditionExpression> Conditions { get; } = new List<ConditionExpression>();
        public List<string> ColumnSet { get; } = new List<string>();
        public override string EntityName { get; }
        public bool IncludeDeactivatedEntities { get; set; }
        public string OrderBy { get; set; }

        #endregion

        #region Methoden

        #region AddCondition

        public void AddCondition(Enum attributeName, ConditionOperator conditionOperator, object value)
        {
            var attr = EntityNameResolver.ResolveAttributeName(EntityName, attributeName.ToString().ToLowerInvariant());
            AddCondition(attr, conditionOperator, value);
        }
        public void AddCondition(string attributeName, ConditionOperator conditionOperator, object value)
        => AddCondition(new ConditionExpression(attributeName, conditionOperator, value));
        public void AddCondition(ConditionExpression conditionExpression)
        {
            Conditions.Add(conditionExpression);
        }

        #endregion

        #region ToOData

        public override string ToOData()
        {
            if (!string.IsNullOrEmpty(NextLink))
            {
                return NextLink;
            }

            var odata = new StringBuilder("$select=");

            if (ColumnSet.Any())
            {
                odata.Append(ColumnSet.ToString(","));
            }
            else
            {
                odata.Append("*");
            }

            if (!string.IsNullOrEmpty(OrderBy))
            {
                odata.Append($"&$orderby={OrderBy}");
            }

            var trimEnd = false;
            if (Conditions.Any())
            {
                odata.Append("&$filter=");

                foreach (var condition in Conditions)
                {
                    odata.Append($"{condition.ToOData()} and ");
                }
                trimEnd = true;
            }
            if (EntityNameResolver.HasStateCodeAttribute(EntityName) &&
                !IncludeDeactivatedEntities)
            {
                if (!Conditions.Any())
                {
                    odata.Append("&$filter=");
                }
                odata.Append("statecode eq 0 and ");
                trimEnd = true;
            }
            if (trimEnd)
            {
                return odata.ToString(0, odata.Length - 4);
            }
            return odata.ToString();
        }

        #endregion

        #endregion

        #region Statische Methoden

        public static QueryExpression Create<T>()
            where T : CitaviCrmEntity
        {
            var entityName = EntityNameResolver.GetEntityLogicalName<T>();
            var expression = new QueryExpression(entityName);
            return expression;
        }

        #endregion
    }
}
