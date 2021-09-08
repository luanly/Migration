using System.ComponentModel;

namespace SwissAcademic.Collections
{
    public class ListChangedEventArgsEx
        :
        ListChangedEventArgs
    {
        #region Felder

        private object _removedObject;

        #endregion

        #region Konstruktoren

        public ListChangedEventArgsEx(ListChangedType listChangedType, int newIndex, int oldIndex) :
            base(listChangedType, newIndex, oldIndex)
        {
        }

        public ListChangedEventArgsEx(int removedIndex, object removedObject) :
            base(ListChangedType.ItemDeleted, removedIndex, -1)
        {
            _removedObject = removedObject;
        }

        #endregion

        #region Eigenschaften

        #region RemovedObject

        public object RemovedObject
        {
            get { return _removedObject; }
        }

        #endregion

        #endregion
    }
}
