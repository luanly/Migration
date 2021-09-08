namespace SwissAcademic.Collections
{
    #region INotifyPropertyChanged

    public interface INotifyPropertyChanged<T>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event PropertyChangedEventHandler<T> PropertyChanged;
    }

    #endregion
}
