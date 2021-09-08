namespace SwissAcademic.Collections
{
    public interface ICollectionChangeRecord
    {
        object Item { get; }
        int NewIndex { get; }
        int OldIndex { get; }
        IPropertyChangedEventArgs Trigger { get; }
    }
}
