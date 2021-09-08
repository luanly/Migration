using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SwissAcademic.Collections
{
    /// <summary>
    /// A thread safe collection
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public abstract class ConcurrentCollection<T> :
        IEnumerable<T>, INotifyCollectionChanged
    {
        #region Fields

        ConcurrentDictionary<T, object> _concurrentDictionary = new ConcurrentDictionary<T, object>();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Properties

        #region SuspendNotification
        public bool SuspendNotification { get; set; }
        #endregion

        #endregion

        #region Methods

        #region AddProtected
        /// <summary>
        /// Adds the item to the collection
        /// </summary>
        /// <param name="item">object of type <see cref="T"/></param>
        protected void AddProtected(T item)
        {
            if (_concurrentDictionary.TryAdd(item, null))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
        }
        #endregion

        #region RemoveProtected
        /// <summary>
        /// Removes the item from the collection
        /// </summary>
        /// <param name="item">item of type <see cref="T"/></param>
        protected void RemoveProtected(T item)
        {
            if (_concurrentDictionary.TryRemove(item, out _))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }
        }
        /// <summary>
        /// Gets whether the <see cref="ConcurrentCollection{T}"/> contains the given item
        /// </summary>
        /// <param name="item">The item to search for</param>
        /// <returns><c>true</c> if the item is already in the collection</returns>
        public bool Contains(T item)
        {
            return _concurrentDictionary.ContainsKey(item);
        }
        /// <summary>
        /// Raises <see cref="CollectionChanged"/>
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (SuspendNotification) return;

            var ev = CollectionChanged;
            if (ev != null)
                Environment.SendEvent(d => ev(this, e));
        }

        #endregion

        #region IEnumerable implementation
        #region GetEnumerator
        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => new ConcurrentCollectionEnumerator<T>(_concurrentDictionary.GetEnumerator());
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => new ConcurrentCollectionEnumerator<T>(_concurrentDictionary.GetEnumerator());

        #endregion
        #endregion

        #endregion
    }
}
