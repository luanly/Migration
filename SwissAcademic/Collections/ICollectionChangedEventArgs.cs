using System.Collections.Generic;

namespace SwissAcademic.Collections
{
    public interface ICollectionChangedEventArgs
    {
        CollectionChangeType ChangeType { get; }
        bool HasRecords { get; }
        IEnumerable<ICollectionChangeRecord> Records { get; }
    }
}
