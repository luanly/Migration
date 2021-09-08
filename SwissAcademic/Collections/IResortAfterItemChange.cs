namespace SwissAcademic.Collections
{
    public interface IResortAfterItemChange<T>
    {
        bool SuppressResort(PropertyChangedEventArgs<T> e);
    }
}
