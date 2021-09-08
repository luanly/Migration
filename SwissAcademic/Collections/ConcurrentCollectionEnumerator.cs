using System.Collections;
using System.Collections.Generic;

namespace SwissAcademic.Collections
{
    #region ConcurrentCollectionEnumerator
    public class ConcurrentCollectionEnumerator<T>
    :
    IEnumerator<T>
    {
        #region Fields

        IEnumerator<KeyValuePair<T, object>> _pairEnumerator;

        #endregion

        #region Constructors

        internal ConcurrentCollectionEnumerator(IEnumerator<KeyValuePair<T, object>> pairEnumerator)
        {
            _pairEnumerator = pairEnumerator;
        }

        #endregion

        #region Properties

        public T Current => _pairEnumerator.Current.Key;

        object IEnumerator.Current => _pairEnumerator.Current.Key;

        #endregion

        #region Methods

        public void Dispose()
        {
            _pairEnumerator.Dispose();
        }

        public bool MoveNext() => _pairEnumerator.MoveNext();

        public void Reset()
        {
            _pairEnumerator.Reset();
        }

        #endregion

        #endregion
    }
}
