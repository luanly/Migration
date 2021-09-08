using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SwissAcademic.Crm.Web
{
    public class ConditionExpression
    {
        #region Felder

        static Dictionary<ConditionOperator, string> _conditionOperators = new Dictionary<ConditionOperator, string>
        {
            {ConditionOperator.Equal, "eq" },
            {ConditionOperator.NotEqual, "ne" },
            {ConditionOperator.GreaterThan, "gt"},
            {ConditionOperator.GreaterEqual, "ge"},
            {ConditionOperator.LessThan, "lt"},
            {ConditionOperator.LessEqual, "le"},
        };

        #endregion

        #region Konstruktor

        public ConditionExpression(string attributeName, ConditionOperator conditionOperator, object value)
        {
            AttributeName = attributeName;
            Operator = conditionOperator;
            if (value is Enum)
            {
                Value = (int)value;
            }
            else
            {
                Value = value;
            }

            if (value is string)
            {
                CrmAttributeValidator.Validate(attributeName, (string)value);
            }
        }

        #endregion

        #region Eigenschaften

        public string AttributeName { get; set; }
        public ConditionOperator Operator { get; set; }
        public object Value { get; set; }

        #endregion

        #region Methoden

        #region ToOData

        public string ToOData()
        {
            var odata = new StringBuilder();
            if (Operator == ConditionOperator.BeginsWith)
            {
                odata.Append($"startswith({AttributeName}, '{Value}')");
            }
            else if (Operator == ConditionOperator.EndsWith)
            {
                odata.Append($"endswith({AttributeName}, '{Value}')");
            }
            else if (Operator == ConditionOperator.Contains)
            {
                odata.Append($"contains({AttributeName}, '{Value}')");
            }
            else
            {
                if (Value is string)
                {
                    odata.Append($"{AttributeName} {_conditionOperators[Operator]} '{Value}'");
                }
                else if (Value is DateTime)
                {
                    odata.Append($"{AttributeName} {_conditionOperators[Operator]} '{((DateTime)Value).ToString("o")}'");
                }
                else
                {
                    odata.Append($"{AttributeName} {_conditionOperators[Operator]} {Value}");
                }
            }
            return odata.ToString();
        }


        #endregion

        #endregion
    }
}
