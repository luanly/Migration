using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace SwissAcademic.Collections
{
    public sealed class BindingCollection<T>
        :
        IList<T>,
        IBindingList,
        IDisposable
        where T : class
    {
        #region Ereignisse

        public event ListChangedEventHandler ListChanged;
        void OnListChanged(ListChangedEventArgs e)
        {
            if (ListChanged == null) return;
            SwissAcademic.Environment.SendEvent(d => ListChanged(this, e));
        }

        public event AddingNewEventHandler AddingNew;
        void OnAddingNew(AddingNewEventArgs e)
        {
            if (AddingNew == null) return;
            SwissAcademic.Environment.SendEvent(d => AddingNew(this, e));
        }

        #endregion

        #region InnerList

        NotificationCollection<T> _innerList;

        #endregion

        #region Konstruktoren

        public BindingCollection(NotificationCollection<T> innerList)
        {
            _innerList = innerList;
            Observe(true);
        }

        #endregion

        #region Eigenschaften

        #region AllowEdit

        public bool AllowEdit { get; set; }

        #endregion

        #region AllowNew

        public bool AllowNew { get; set; }

        #endregion

        #region AutoSort

        public bool AutoSort
        {
            get { return _innerList.AutoSort; }
            set { _innerList.AutoSort = value; }
        }

        #endregion

        #region Count

        public int Count
        {
            get { return _innerList.Count; }
        }

        #endregion

        #region IsNotificationSuspended

        public bool IsNotificationSuspended
        {
            get { return _innerList.IsNotificationSuspended; }
        }

        #endregion

        #region IsReadOnly

        public bool IsReadOnly
        {
            get { return _innerList.IsReadOnly; }
        }

        #endregion

        #region NotifyItemChanges

        public bool NotifyItemChanges
        {
            get { return _innerList.NotifyItemChanges; }
        }

        #endregion

        #region this

        public T this[int index]
        {
            get { return _innerList[index]; }
            set { _innerList[index] = value; }
        }

        #endregion

        #endregion

        #region Methoden

        #region Add

        public void Add(T item)
        {
            _innerList.Add(item);
        }

        #endregion

        #region AddNew

        T _newItem;

        public T AddNew()
        {
            if (!AllowNew) throw new NotSupportedException();

            var e = new AddingNewEventArgs();
            OnAddingNew(e);


            _newItem = e.NewObject as T;
            if (_newItem != null) Add(_newItem);

            return _newItem;
        }

        #endregion

        #region AddRange

        public void AddRange(IEnumerable<T> collection)
        {
            _innerList.AddRange(collection);
        }

        #endregion

        #region AllowMove

        public bool AllowMove
        {
            get { return _innerList.AllowMove; }
        }

        #endregion

        #region AllowRemove

        public bool AllowRemove { get; set; }

        #endregion

        #region CancelNew

        public void CancelNew()
        {
            if (_newItem != null) Remove(_newItem);
        }

        #endregion

        #region Clear

        public void Clear()
        {
            _innerList.Clear();
        }

        #endregion

        #region Contains

        public bool Contains(T item)
        {
            return _innerList.Contains(item);
        }

        #endregion

        #region CopyTo

        public void CopyTo(T[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                Observe(false);
            }
        }

        #endregion

        #region EndNew

        public void EndNew()
        {
            _newItem = null;
        }

        #endregion

        #region GetEnumerator

        public IEnumerator<T> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        #endregion

        #region IndexOf

        public int IndexOf(T item)
        {
            return _innerList.IndexOf(item);
        }

        #endregion

        #region Insert

        public void Insert(int index, T item)
        {
            _innerList.Insert(index, item);
        }

        #endregion

        #region InsertRange

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            _innerList.InsertRange(index, collection);
        }

        #endregion

        #region Move

        public void Move(T moveItem, T insertAfterItem)
        {
            _innerList.Move(moveItem, insertAfterItem);
        }

        #endregion

        #region MoveRange

        public void MoveRange(IEnumerable<T> moveCollection, T insertAfterItem)
        {
            _innerList.MoveRange(moveCollection, insertAfterItem);
        }

        #endregion

        #region Observe

        void Observe(bool start)
        {
            if (_innerList == null) return;

            if (start) _innerList.CollectionChanged += innerList_CollectionChanged;
            else _innerList.CollectionChanged -= innerList_CollectionChanged;
        }

        #endregion

        #region RaiseReset

        public void RaiseReset()
        {
            _innerList.RaiseReset();
        }

        #endregion

        #region ReleaseEvents

        public void ReleaseEvents()
        {
            AddingNew = null;
            ListChanged = null;
        }

        #endregion

        #region Remove

        public void Remove(T item)
        {
            _innerList.Remove(item);
        }

        #endregion

        #region RemoveAt

        public void RemoveAt(int index)
        {
            _innerList.RemoveAt(index);
        }

        #endregion

        #region RemoveRange

        public void RemoveRange(IEnumerable<T> collection)
        {
            _innerList.RemoveRange(collection);
        }

        #endregion

        #region ReplaceBy

        public void ReplaceBy(IEnumerable<T> collection)
        {
            _innerList.ReplaceBy(collection);
        }

        #endregion

        #region ResumeNotification

        public void ResumeNotification()
        {
            _innerList.ResumeNotification();
        }

        public void ResumeNotification(bool raisePendingEvents)
        {
            _innerList.ResumeNotification(raisePendingEvents);
        }

        #endregion

        #region Sort

        public void Sort()
        {
            _innerList.Sort();
        }

        public void Sort(Comparison<T> comparison)
        {
            _innerList.Sort(comparison);
        }

        public void Sort(IComparer<T> comparer)
        {
            _innerList.Sort(comparer);
        }

        #endregion

        #region SuspendNotification

        public void SuspendNotification()
        {
            _innerList.SuspendNotification();
        }

        #endregion

        #region ToArray

        public T[] ToArray()
        {
            return _innerList.ToArray();
        }

        #endregion

        #endregion

        #region Ereignishandler

        #region InnerList

        void innerList_CollectionChanged(object sender, CollectionChangedEventArgs<T> e)
        {
            if (!e.HasRecords)
            {
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
                return;
            }

            switch (e.ChangeType)
            {
                case CollectionChangeType.ItemsAdded:
                    {
                        foreach (var record in e.Records)
                        {
                            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, record.NewIndex));
                        }
                    }
                    break;

                case CollectionChangeType.ItemsChanged:
                    {
                        foreach (var record in e.Records)
                        {
                            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, record.NewIndex, record.OldIndex));
                        }
                    }
                    break;

                case CollectionChangeType.ItemsDeleted:
                    {
                        foreach (var record in e.Records)
                        {
                            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, record.NewIndex, record.OldIndex));
                        }
                    }
                    break;

                case CollectionChangeType.ItemsMoved:
                    {
                        foreach (var record in e.Records)
                        {
                            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemMoved, record.NewIndex, record.OldIndex));
                        }
                    }
                    break;

                case CollectionChangeType.Reset:
                case CollectionChangeType.Sorted:
                    OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
                    break;
            }
        }

        #endregion

        #endregion

        #region IList<T> Members

        bool ICollection<T>.Remove(T item)
        {
            return ((IList<T>)_innerList).Remove(item);
        }

        #endregion

        #region IBindingList Members

        void IBindingList.AddIndex(PropertyDescriptor property)
        {
            // http://msdn.microsoft.com/en-us/library/bb302762(v=VS.100).aspx
            // The base implementation of this method does nothing. If this functionality is desired, a derived class must implement it.
        }

        object IBindingList.AddNew()
        {
            return AddNew();
        }

        bool IBindingList.AllowEdit
        {
            get { return true; }
        }

        bool IBindingList.AllowNew
        {
            get { return true; }
        }

        bool IBindingList.AllowRemove
        {
            get { return true; }
        }

        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            throw new NotSupportedException();
        }

        int IBindingList.Find(PropertyDescriptor property, object key)
        {
            throw new NotSupportedException();
        }

        bool IBindingList.IsSorted
        {
            get { throw new NotSupportedException(); }
        }

        void IBindingList.RemoveIndex(PropertyDescriptor property)
        {
            // http://msdn.microsoft.com/en-us/library/bb354914(v=VS.100).aspx
            // The base implementation of this method does nothing. If this functionality is desired, a derived class must implement it.
        }

        void IBindingList.RemoveSort()
        {
            throw new NotSupportedException();
        }

        ListSortDirection IBindingList.SortDirection
        {
            get { throw new NotSupportedException(); }
        }

        PropertyDescriptor IBindingList.SortProperty
        {
            get { throw new NotSupportedException(); }
        }

        bool IBindingList.SupportsChangeNotification
        {
            get { return true; }
        }

        bool IBindingList.SupportsSearching
        {
            get { return false; }
        }

        bool IBindingList.SupportsSorting
        {
            get { return false; }
        }

        #endregion

        #region IList Members

        int System.Collections.IList.Add(object value)
        {
            var item = (T)value;
            Add(item);
            return _innerList.IndexOf(item);
        }

        bool System.Collections.IList.Contains(object value)
        {
            return Contains((T)value);
        }

        int System.Collections.IList.IndexOf(object value)
        {
            return _innerList.IndexOf((T)value);
        }

        void System.Collections.IList.Insert(int index, object value)
        {
            _innerList.Insert(index, (T)value);
        }

        bool System.Collections.IList.IsFixedSize
        {
            get { return IsReadOnly; }
        }

        void System.Collections.IList.Remove(object value)
        {
            Remove((T)value);
        }

        object System.Collections.IList.this[int index]
        {
            get { return _innerList[index]; }
            set { _innerList[index] = (T)value; }
        }

        #endregion

        #region ICollection Members

        void System.Collections.ICollection.CopyTo(Array array, int index)
        {
            CopyTo((T[])array, index);
        }

        bool System.Collections.ICollection.IsSynchronized
        {
            get { return ((ICollection)_innerList).IsSynchronized; }
        }

        object System.Collections.ICollection.SyncRoot
        {
            get { return ((ICollection)_innerList).SyncRoot; }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
