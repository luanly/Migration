using System;
using System.Collections.Generic;
using System.Linq;

namespace SwissAcademic.Collections
{
    [Serializable]
    public sealed class GroupDescriptorCollection<T>
        :
        IList<GroupDescriptor<T>>
    {
        #region Felder

        List<GroupDescriptor<T>> _innerList;

        #endregion

        #region Konstruktoren

        public GroupDescriptorCollection()
        {
            _innerList = new List<GroupDescriptor<T>>();
        }

        public GroupDescriptorCollection(int capacity)
        {
            _innerList = new List<GroupDescriptor<T>>(capacity);
        }

        #endregion

        #region Eigenschaften

        #region Count

        public int Count
        {
            get { return _innerList.Count; }
        }

        #endregion

        #region this

        public GroupDescriptor<T> this[int index]
        {
            get { return _innerList[index]; }
            set { throw new NotSupportedException(); }
        }

        public GroupDescriptor<T> this[PropertyDescriptor<T> property]
        {
            get { return _innerList.Find(item => item.Property == property); }
        }

        #endregion

        #endregion

        #region Methoden

        #region Add

        public void Add(GroupDescriptor<T> item)
        {
            Insert(Count, item);
        }

        public GroupDescriptor<T> Add(PropertyDescriptor<T> property)
        {
            return Insert(Count, property);
        }

        #endregion

        #region Clear

        public void Clear()
        {
            _innerList.Clear();
        }

        #endregion

        #region Contains

        public bool Contains(GroupDescriptor<T> item)
        {
            return _innerList.Contains(item);
        }

        public bool Contains(PropertyDescriptor<T> property)
        {
            return _innerList.Any(item => item.Property == property);
        }

        #endregion

        #region IndexOf

        public int IndexOf(GroupDescriptor<T> item)
        {
            return _innerList.IndexOf(item);
        }

        public int IndexOf(PropertyDescriptor<T> property)
        {
            return _innerList.FindIndex(item => item.Property == property);
        }

        #endregion

        #region Insert

        public void Insert(int index, GroupDescriptor<T> item)
        {
            if (!Contains(item.Property)) _innerList.Insert(index, item);
        }

        public GroupDescriptor<T> Insert(int index, PropertyDescriptor<T> property)
        {
            var item = this[property];
            if (item != null) return item;

            item = new GroupDescriptor<T>(property);
            Insert(index, item);
            return item;
        }

        #endregion

        #region Remove

        public bool Remove(GroupDescriptor<T> item)
        {
            return _innerList.Remove(item);
        }

        public bool Remove(PropertyDescriptor<T> property)
        {
            var item = this[property];
            if (item == null) return false;
            return Remove(item);
        }

        #endregion

        #region RemoveAt

        public void RemoveAt(int index)
        {
            _innerList.RemoveAt(index);
        }

        #endregion

        #endregion

        #region IList<GroupDescriptor<T>> Members

        void ICollection<GroupDescriptor<T>>.CopyTo(GroupDescriptor<T>[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        bool ICollection<GroupDescriptor<T>>.IsReadOnly
        {
            get { return false; }
        }

        IEnumerator<GroupDescriptor<T>> IEnumerable<GroupDescriptor<T>>.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        #endregion
    }
}
