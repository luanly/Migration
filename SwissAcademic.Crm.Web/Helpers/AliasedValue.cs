namespace SwissAcademic.Crm.Web
{
    public sealed class AliasedValue
    {
        public AliasedValue(string entityLogicalName, string attributeLogicalName, object value)
        {
            AttributeLogicalName = attributeLogicalName;
            EntityLogicalName = entityLogicalName;
            Value = value;
        }
        public string AttributeLogicalName { get; }
        public string EntityLogicalName { get; }
        public object Value { get; }
    }
}
