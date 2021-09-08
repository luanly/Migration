namespace SwissAcademic.Collections
{
    public interface IPropertyChangedEventArgs
    {
        ICollectionChangedEventArgs CollectionChangedTrigger { get; }
        bool IsReset { get; }
        IPropertyDescriptor Property { get; }
        IPropertyChangedEventArgs PropertyChangedTrigger { get; }
    }
}
