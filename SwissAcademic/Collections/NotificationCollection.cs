using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SwissAcademic.Collections
{
    #region SuspensionRecord

    [Serializable]
    internal class SuspensionRecord<T>
    {

        #region Konstruktoren

        public SuspensionRecord(CollectionChangeType changeType, CollectionChangeRecord<T> record)
        {
            ChangeType = changeType;
            Record = record;
        }

        #endregion

        #region Eigenschaften

        #region ChangeType

        public CollectionChangeType ChangeType { get; private set; }

        #endregion

        #region Record

        public CollectionChangeRecord<T> Record { get; private set; }

        #endregion

        #endregion
    }

    #endregion

    [Serializable]
    public class NotificationCollection<T>
        :
        IList<T>,
        System.Collections.IList
    {
        object _lockObj = new object();

        #region Ereignisse

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        [field: NonSerialized]
        public event CollectionChangingEventHandler<T> AfterRemoveRange;
        protected virtual void OnAfterRemoveRange(CollectionChangingEventArgs<T> e)
        {
            var afterRemoveRange = AfterRemoveRange;
            if (afterRemoveRange != null) Environment.SendEvent(d => afterRemoveRange(this, e));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        [field: NonSerialized]
        public event CollectionChangingEventHandler<T> BeforeRemoveRange;
        protected virtual void OnBeforeRemoveRange(CollectionChangingEventArgs<T> e)
        {
            var beforeRemoveRange = BeforeRemoveRange;
            if (beforeRemoveRange != null) Environment.SendEvent(d => beforeRemoveRange(this, e));
        }

        [field: NonSerialized]
        public event CollectionChangedEventHandler<T> CollectionChanged;

        protected virtual void OnCollectionChanged(CollectionChangedEventArgs<T> e)
        {
            var collectionChanged = CollectionChanged;
            if (collectionChanged != null)
            {
                Environment.SendEvent(d => collectionChanged(this, e));
            }
        }

        #endregion

        #region Felder

        HashSet<T> _duplicatesCheckSet;
        List<T> _innerList;

        #endregion

        #region Konstruktoren

        public NotificationCollection()
        {
            if (typeof(INotifyPropertyChanged<T>).IsAssignableFrom(typeof(T))) SupportsNotifyItemChanges = true;
            _innerList = new List<T>();
        }

        public NotificationCollection(int capacity)
        {
            if (typeof(INotifyPropertyChanged<T>).IsAssignableFrom(typeof(T))) SupportsNotifyItemChanges = true;
            _innerList = new List<T>(capacity);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NotificationCollection(IEnumerable<T> collection)
        {
            if (typeof(INotifyPropertyChanged<T>).IsAssignableFrom(typeof(T))) SupportsNotifyItemChanges = true;
            InitializeBulk(collection);
        }

        public NotificationCollection(bool isReadOnly, bool allowDuplicates, bool notifyItemChanges, bool allowMove)
            :
            this()
        {
            IsReadOnly = isReadOnly;
            AllowDuplicates = allowDuplicates;
            _notifyItemChanges = notifyItemChanges;
            AllowMove = allowMove;
        }

        public NotificationCollection(bool isReadOnly, bool allowDuplicates, bool notifyItemChanges, bool allowMove, int capacity)
            :
            this(capacity)
        {
            IsReadOnly = isReadOnly;
            AllowDuplicates = allowDuplicates;
            _notifyItemChanges = notifyItemChanges;
            AllowMove = allowMove;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public NotificationCollection(bool isReadOnly, bool allowDuplicates, bool notifyItemChanges, bool allowMove, IEnumerable<T> collection)
        {
            if (typeof(INotifyPropertyChanged<T>).IsAssignableFrom(typeof(T))) SupportsNotifyItemChanges = true;

            IsReadOnly = isReadOnly;
            AllowDuplicates = allowDuplicates;
            _notifyItemChanges = notifyItemChanges;
            AllowMove = allowMove;

            if (!allowDuplicates && collection != null) collection = collection.Distinct();

            InitializeBulk(collection);
        }

        internal NotificationCollection(NotificationCollection<T> sourceCollection, Func<T, bool> filter)
            :
            this(true, sourceCollection.AllowDuplicates, sourceCollection.NotifyItemChanges, sourceCollection.AllowMove, filter == null ? sourceCollection : from item in sourceCollection where filter(item) select item)
        {
            SourceCollection = sourceCollection;
            Filter = filter;

            ObserveSourceCollection(true);
        }

        #endregion

        #region Eigenschaften

        #region AddNextItemAtIndex

        [NonSerialized]
        int _addNextItemAtIndex = -1;

        public int AddNextItemAtIndex
        {
            get { return _addNextItemAtIndex; }
            set { _addNextItemAtIndex = value; }
        }

        #endregion

        #region AllowDuplicates

        public bool AllowDuplicates { get; private set; }

        #endregion

        #region AllowMove

        public bool AllowMove { get; private set; }

        #endregion

        #region AutoSort

        bool _autoSort;

        public bool AutoSort
        {
            get { return _autoSort; }

            set
            {
                if (value == _autoSort) return;

                _autoSort = value;
                if (_autoSort) Sort();
            }
        }

        #endregion

        #region Comparer

        /* DEFFERED -> Custom-Serialisierung prüfen
		 * Es wäre wohl sinnvoll, wenn dieses Feld serializable bleibt, damit es beim Serialisieren mitkopiert wird.
		 * Aber was passiert, wenn die Comparer-Implementation nicht serializable ist? Vermutlich scheitert dann die
		 * ganze Serialisierung.
		 * Können wir ev. mit einer Custom-Serialisierung prüfen, ob die Implementation serialisierbar ist und 
		 * angepasst reagieren? */
        IComparer<T> _comparer;

        public IComparer<T> Comparer
        {
            get
            {
                if (_comparer == null && SourceCollection != null)
                {
                    return SourceCollection.Comparer;
                }
                return _comparer;
            }
            set
            {
                if (value != _comparer)
                {
                    _comparer = value;
                    if (_autoSort) Sort();
                }
            }
        }

        #endregion

        #region Count

        public int Count
        {
            get { return _innerList.Count; }
        }

        #endregion

        #region Filter

        public Func<T, bool> Filter { get; private set; }

        #endregion

        #region IsNotificationSuspended

        public bool IsNotificationSuspended
        {
            get { return _notificationSuspendedCounter != 0; }
        }

        #endregion

        #region IsReadOnly

        public bool IsReadOnly { get; private set; }

        #endregion

        #region MaxChangeNotificationGenerations

        int _maxChangeNotificationGenerations = 1;

        public int MaxChangeNotificationGenerations
        {
            get
            {
                if (!_notifyItemChanges) return 0;
                return _maxChangeNotificationGenerations;
            }
            set
            {
                value = Math.Max(0, value);
                _maxChangeNotificationGenerations = value;
            }
        }

        #endregion

        #region NotifyItemChanges

        bool _notifyItemChanges;

        public bool NotifyItemChanges
        {
            get { return _notifyItemChanges && SupportsNotifyItemChanges; }
        }

        #endregion

        #region SourceCollection

#pragma warning disable CA5362
        public NotificationCollection<T> SourceCollection { get; private set; }
#pragma warning restore CA5362

        #endregion

        #region SupportsNotifyItemChanges

        public bool SupportsNotifyItemChanges { get; private set; }

        #endregion

        #region SuspensionRecords

        [NonSerialized]
        ConcurrentQueue<SuspensionRecord<T>> _suspensionRecords;

        ConcurrentQueue<SuspensionRecord<T>> SuspensionRecords => LazyInitializer.EnsureInitialized(ref _suspensionRecords);

        #endregion

        #region this

        public T this[int index]
        {
            get { return _innerList[index]; }
            set { throw new NotImplementedException(); }
        }

        #endregion

        #endregion

        #region Methoden

        #region Add

        public void Add(T item)
        {
            if (IsReadOnly) throw new InvalidOperationException("This collection is read only");
            AddItem(item, true);
        }

        #endregion

        #region AddItem

        protected void AddItem(T item, bool performChecks)
        {
            if (_addNextItemAtIndex < 0 || _addNextItemAtIndex > Count)
            {
                InsertItem(Count, item, performChecks);
            }
            else
            {
                InsertItem(_addNextItemAtIndex, item, performChecks);
            }
        }

        #endregion

        #region AddItemRange

        protected void AddItemRange(IEnumerable<T> collection, bool performChecks)
        {
            if (_addNextItemAtIndex < 0 || _addNextItemAtIndex > Count)
            {
                InsertItemRange(Count, collection, performChecks);
            }
            else
            {
                InsertItemRange(AddNextItemAtIndex, collection, performChecks);
            }
        }

        #endregion

        #region AddRange

        public void AddRange(IEnumerable<T> collection)
        {
            if (_addNextItemAtIndex < 0 || _addNextItemAtIndex > Count)
            {
                InsertRange(Count, collection);
            }
            else
            {
                InsertRange(AddNextItemAtIndex, collection);
            }
        }

        #endregion

        #region AddToDuplicateHashSet

        void AddToDuplicateHashSet(T item)
        {
            if (_duplicatesCheckSet.Contains(item))
            {
                Telemetry.TrackTrace($"Adding item \"{item}\" to this collection failed since {nameof(AllowDuplicates)} is false and the item is already in the collection.", property1: ("item", item));
                throw new ArgumentException($"Adding item \"{item}\" to this collection failed since {nameof(AllowDuplicates)} is false and the item is already in the collection.");
            }
            else
            {
                _duplicatesCheckSet.Add(item);
            }
        }

        void AddToDuplicateHashSet(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                AddToDuplicateHashSet(item);
            }
        }

        #endregion

        #region AsFiltered

        public NotificationCollection<T> AsFiltered(Func<T, bool> filter)
        {
            return new NotificationCollection<T>(this, filter);
        }

        #endregion

        #region CalculateGenerations

        void CalculateGenerations(ICollectionChangedEventArgs e, ref int generation)
        {
            if (e.ChangeType == CollectionChangeType.ItemsChanged)
            {
                generation++;
                if (e.Records.First().Trigger.CollectionChangedTrigger != null) CalculateGenerations(e.Records.First().Trigger.CollectionChangedTrigger, ref generation);
            }
        }

        #endregion

        #region Clear

        public virtual void Clear()
        {
            if (IsReadOnly) throw new InvalidOperationException("This collection is read only");

            /* RemoveRange aufrufen statt _innerList direkt leeren, um überschriebene
			 * Implementationen von RemoveItemRange zu berücksichtigen
			 * */
            RemoveItemRange(this.ToList(), false);
        }

        #endregion

        #region Contains

        public virtual bool Contains(T item)
        {
            return _duplicatesCheckSet == null ?
                _innerList.Contains(item) :
                _duplicatesCheckSet.Contains(item);
        }

        #endregion

        #region CopyTo

        public void CopyTo(T[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        #endregion

        #region CopyEventHandlersTo

        public void CopyEventHandlersTo(NotificationCollection<T> other)
        {
            if (CollectionChanged != null)
            {
                foreach (CollectionChangedEventHandler<T> eventHandler in CollectionChanged.GetInvocationList())
                {
                    other.CollectionChanged += eventHandler;
                }
            }

            if (BeforeRemoveRange != null)
            {
                foreach (CollectionChangingEventHandler<T> eventHandler in BeforeRemoveRange.GetInvocationList())
                {
                    other.BeforeRemoveRange += eventHandler;
                }
            }

            if (AfterRemoveRange != null)
            {
                foreach (CollectionChangingEventHandler<T> eventHandler in AfterRemoveRange.GetInvocationList())
                {
                    other.AfterRemoveRange += eventHandler;
                }
            }
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

        #region InitializeBulk

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void InitializeBulk(IEnumerable<T> items)
        {
            InitializeBulk(items, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void InitializeBulk(IEnumerable<T> items, bool autoSorted)
        {
            if (items == null)
            {
                _innerList = new List<T>();
                return;
            }

            if (_innerList != null && _innerList.Any())
            {
                // Don't change the word "InitializeBulk", the UI evaluates the message text for this word.
                throw new InvalidOperationException("InitializeBulk cannot be called if the collection contains items.");
            }
            _innerList = items.ToList();

            if (_autoSort || (SourceCollection != null && SourceCollection.AutoSort)) _innerList.Sort(Comparer);
            if (NotifyItemChanges)
            {
                foreach (var item in _innerList)
                {
                    Observe(item, true);
                }
            }

            if (!AllowDuplicates && Count > CollectionUtility.DuplicatesCheckHashSetThreshold)
            {
                _duplicatesCheckSet = new HashSet<T>();
                AddToDuplicateHashSet(_innerList);
            }

            if (autoSorted) _autoSort = true;

            OnAfterInitializeBulk();
        }

        #endregion

        #region Insert

        public void Insert(int index, T item)
        {
            if (IsReadOnly) throw new InvalidOperationException("This collection is read only");
            InsertItem(index, item, true);
        }

        #endregion

        #region InsertItem

        protected virtual void InsertItem(int index, T item, bool performChecks)
        {
            if (performChecks)
            {
                if (item == null) throw new ArgumentNullException();
                if (!AllowDuplicates && Contains(item)) return;
            }

            index = InsertToInnerList(index, item);
        }

        #endregion

        #region InsertRange

        public virtual void InsertRange(int index, IEnumerable<T> collection)
        {
            if (IsReadOnly) throw new InvalidOperationException("This collection is read only");
            InsertItemRange(index, collection, true);
        }

        #endregion

        #region InsertItemRange

        protected virtual void InsertItemRange(int index, IEnumerable<T> collection, bool performChecks)
        {
            if (collection == null) throw new ArgumentNullException();
            if (collection.Any(item => item == null)) throw new ArgumentException();

            if (!AllowDuplicates)
            {
                collection = collection.Distinct();
                collection = from item in collection
                             where !Contains(item)
                             select item;
            }

            try
            {
                SuspendNotification();

                var i = 0;
                foreach (var item in collection)
                {
                    InsertItem(index + i, item, false);
                    i++;
                }
            }

            finally
            {
                ResumeNotification();
            }
        }

        #endregion

        #region InsertToInnerList

        int InsertToInnerList(int index, T item, bool isMove = false, PropertyChangedEventArgs<T> e = null, int oldIndex = -1)
        {
            if (isMove)
            {
                OnBeforeMoveInsert(index, item);
            }
            else
            {
                OnBeforeInsert(index, item);

                if (_autoSort || (SourceCollection != null && SourceCollection.AutoSort))
                {
                    index = _innerList.BinarySearch(item, Comparer);
                    if (index < 0) index = ~index;
                }

                if (NotifyItemChanges) Observe(item, true);

                if (!AllowDuplicates && _duplicatesCheckSet == null && Count > CollectionUtility.DuplicatesCheckHashSetThreshold)
                {
                    _duplicatesCheckSet = new HashSet<T>();
                    AddToDuplicateHashSet(_innerList);
                }

                if (_duplicatesCheckSet != null) AddToDuplicateHashSet(item);
            }

            _innerList.Insert(Math.Min(index, _innerList.Count), item);

            if (isMove)
            {
                // In most cases, newIndex should not equal oldIndex. However, this may happen and is OK if
                // two entries have identical compare values, i.e. two different references with the same
                // short title.
                //Debug.Assert(newIndex != oldIndex);

                if (index != oldIndex)
                {
                    Debug.Assert(oldIndex != -1);

                    if (IsNotificationSuspended)
                    {
                        SuspensionRecords.Enqueue(new SuspensionRecord<T>(CollectionChangeType.ItemsMoved, new CollectionChangeRecord<T>(item, index, oldIndex)));
                    }
                    else OnCollectionChanged(CollectionChangedEventArgs<T>.ItemMoved(item, index, oldIndex, e));
                }

                OnAfterMoveInsert(index, item);
            }

            else
            {
                /* BUG 5021 was caused for the following reason:
				 * 
				 * When a CategoryHierarchyCollection changes (e.g. by adding a new subcategory), the event is 
				 * forwarded to the project's AllCategories collection. The AllCategories collection raises an event
				 * like, for example, "Added".
				 * 
				 * In the OnAfterInsert() override of CategoryHierarchyCollection, the new subcategory is added to the 
				 * project's AllCategories collection.
				 * 
				 * However, OnAfterInsert() here was called *after* OnCollectionChanged. So the AllCategories.CollectionChanged
				 * event was raised *before* the category was added to the AllCategories collection.
				 * 
				 * I have therefore changed the order: OnAfterInsert is called first, afterwords the event is raised.
				 * TS, 7.2.2011 
				 * 
				 * UPDATE: 
				 * Calling OnAfterInsert first has caused problems when a contribution was added to a reference:
				 * In OnAfterInsert, the parent reference is notified that there is a new contribution. When notification
				 * was suspended, this lead toa wrong order of events since the parent's change notification came before 
				 * the insertion notification of the contribution. 
				 * Solution: a new overridable methode OnAfterInsertBeforeChangeNotification which is currently exclusively
				 * used by the above mentioned categories collections.
				 * TS, 20.6.2011
				 */

                OnAfterInsertBeforeChangeNotification(index, item);

                if (IsNotificationSuspended)
                {
                    SuspensionRecords.Enqueue(new SuspensionRecord<T>(CollectionChangeType.ItemsAdded, new CollectionChangeRecord<T>(item, index, -1)));
                }
                else
                {
                    OnCollectionChanged(CollectionChangedEventArgs<T>.ItemAdded(item, index));
                }

                OnAfterInsertAfterChangeNotification(index, item);
            }

            return index;
        }

        #endregion

        #region HasEventHandlersAttached

        public bool HasEventHandlersAttached
        {
            get
            {
                return
                    (CollectionChanged != null && CollectionChanged.GetInvocationList().Any()) ||
                    (BeforeRemoveRange != null && BeforeRemoveRange.GetInvocationList().Any()) ||
                    (AfterRemoveRange != null && AfterRemoveRange.GetInvocationList().Any());
            }
        }

        #endregion

        #region Move

        public void Move(T moveItem, T insertAfterItem)
        {
            if (SourceCollection == null && !AllowMove) throw new InvalidOperationException("This collection does not allow moving.");

            MoveItem(moveItem, insertAfterItem);
        }

        public void MoveRange(IEnumerable<T> moveCollection, T insertAfterItem)
        {
            if (SourceCollection == null && !AllowMove) throw new InvalidOperationException("This collection does not allow moving.");

            if (moveCollection == null) throw new ArgumentNullException();
            if (moveCollection.Any(item => item == null || !Contains(item))) throw new ArgumentException();

            SuspendNotification();

            var orderedEnumerable = moveCollection.OrderBy(item => IndexOf(item));

            foreach (var item in orderedEnumerable)
            {
                Move(item, insertAfterItem);
                insertAfterItem = item;
            }

            ResumeNotification(true);
        }

        #endregion

        #region MoveItem

        protected virtual void MoveItem(T moveItem, T insertAfterItem)
        {
            if (moveItem == null) throw new ArgumentNullException(nameof(moveItem));
            if (moveItem.Equals(insertAfterItem)) return;

            var oldIndex = IndexOf(moveItem);
            if (oldIndex == -1) throw new ArgumentOutOfRangeException("moveItem is not an item of this collection.");
            var newIndex = IndexOf(insertAfterItem) + 1;

            if (oldIndex == newIndex) return;
            if (oldIndex < newIndex) newIndex--;

            RemoveFromInnerList(oldIndex, moveItem, true);
            InsertToInnerList(newIndex, moveItem, true, null, oldIndex);
        }

        #endregion

        #region Observe

        void Observe(T item, bool start)
        {
            System.Diagnostics.Debug.Assert(NotifyItemChanges);

            if (start)
            {
                if (SupportsNotifyItemChanges)
                {
                    var ev = (INotifyPropertyChanged<T>)item;
                    if (ev != null) ev.PropertyChanged += item_PropertyChanged;
                }
            }
            else
            {
                if (SupportsNotifyItemChanges)
                {
                    var ev = (INotifyPropertyChanged<T>)item;
                    if (ev != null) ev.PropertyChanged -= item_PropertyChanged;
                }
            }
        }

        bool _observingSourceCollection;
        void ObserveSourceCollection(bool start)
        {
            if (start && !_observingSourceCollection)
            {
                _observingSourceCollection = true;
                SourceCollection.CollectionChanged += sourceCollection_CollectionChanged;
            }
            else if (!start && _observingSourceCollection)
            {
                _observingSourceCollection = false;
                SourceCollection.CollectionChanged -= sourceCollection_CollectionChanged;
            }
        }

        #endregion

        #region OnAfterInitializeBulk

        protected virtual void OnAfterInitializeBulk()
        {
        }

        #endregion

        #region OnAfterInsertAfterChangeNotification

        protected virtual void OnAfterInsertAfterChangeNotification(int index, T item)
        {
        }

        #endregion

        #region OnAfterInsertBeforeChangeNotification

        protected virtual void OnAfterInsertBeforeChangeNotification(int index, T item)
        {
        }

        #endregion

        #region OnAfterMoveInsert

        protected virtual void OnAfterMoveInsert(int index, T item)
        {
        }

        #endregion

        #region OnAfterMoveRemove

        protected virtual void OnAfterMoveRemove(T item)
        {
        }

        #endregion

        #region OnAfterRemove

        protected virtual void OnAfterRemove(T item)
        {
        }

        #endregion

        #region OnBeforeInsert

        protected virtual void OnBeforeInsert(int index, T item)
        {
        }

        #endregion

        #region OnBeforeMove

        protected virtual void OnBeforeMoveInsert(int index, T item)
        {
        }

        protected virtual void OnBeforeMoveRemove(T item)
        {
        }

        #endregion

        #region OnBeforeRemove

        protected virtual void OnBeforeRemove(T item)
        {
        }

        #endregion

        #region OnBeforeSorted

        protected virtual void OnBeforeSorted()
        {
        }

        #endregion

        #region OnAfterReset

        protected virtual void OnAfterReset()
        {
        }

        #endregion

        #region OnAfterSorted

        protected virtual void OnAfterSorted()
        {
        }

        #endregion

        #region RaiseReset

        public void RaiseReset()
        {
            if (!IsNotificationSuspended)
            {
                OnCollectionChanged(CollectionChangedEventArgs<T>.Reset());
            }
        }

        #endregion

        #region ReleaseEvents

        public void ReleaseEvents()
        {
            if (NotifyItemChanges) foreach (var item in this) Observe(item, false);
            CollectionChanged = null;
            BeforeRemoveRange = null;
            AfterRemoveRange = null;

            if (SourceCollection != null)
            {
                ObserveSourceCollection(false);
                SourceCollection = null;
                Filter = null;
            }
        }

        #endregion

        #region Remove

        public void Remove(T item)
        {
            if (IsReadOnly) throw new InvalidOperationException("This collection is read only");
            if (item == null) return;

            // Call RemoveItemRange to ensure that the "BeforeRemove" event is called
            // in all cases. 
            // Before, we had an inconsistent behaviour: if RemoveRange(items) was called,
            // the event was raised, if Remove(item) was called, the event was not raised.
            // This led to BUG 5955 because an event handler was relying on the BeforeRemoveRange event.
            RemoveItemRange(new List<T> { item }, true);
        }

        #endregion

        #region RemoveItem

        protected virtual void RemoveItem(T item)
        {
            int index = IndexOf(item);
            if (index == -1) return;

            RemoveItem(index, item);
        }

        void RemoveItem(int index, T item)
        {
            RemoveFromInnerList(index, item);

            if (IsNotificationSuspended)
            {
                SuspensionRecords.Enqueue(new SuspensionRecord<T>(
                    CollectionChangeType.ItemsDeleted,
                    new CollectionChangeRecord<T>(item, index, -1)));
            }
            else
            {
                OnCollectionChanged(CollectionChangedEventArgs<T>.ItemDeleted(item, index));
            }
        }

        #endregion

        #region RemoveAt

        public void RemoveAt(int index)
        {
            if (IsReadOnly) throw new InvalidOperationException("This collection is read only");
            RemoveItem(index, this[index]);
        }

        protected void RemoveItemAt(int index)
        {
            RemoveItem(index, this[index]);
        }

        #endregion

        #region RemoveFromInnerList

        void RemoveFromInnerList(int oldIndex, T item, bool isMove = false)
        {
            if (isMove)
            {
                OnBeforeMoveRemove(item);
            }

            else
            {
                OnBeforeRemove(item);
                if (NotifyItemChanges) Observe(item, false);
                if (_duplicatesCheckSet != null) _duplicatesCheckSet.Remove(item);
            }

            _innerList.RemoveAt(oldIndex);

            if (isMove) OnAfterMoveRemove(item);
            else OnAfterRemove(item);
        }

        #endregion

        #region RemoveRange

        public void RemoveRange(IEnumerable<T> collection)
        {
            if (IsReadOnly) throw new InvalidOperationException("This collection is read only");
            RemoveItemRange(collection, true);
        }

        #endregion

        #region RemoveItemRange

        protected virtual void RemoveItemRange(IEnumerable<T> collection, bool performChecks)
        {
            if (performChecks)
            {
                if (collection == null) throw new ArgumentNullException();
            }

            if (!collection.Any()) return;

            try
            {
                OnBeforeRemoveRange(new CollectionChangingEventArgs<T>(collection));

                SuspendNotification();

                foreach (var item in collection.ToList())
                {
                    RemoveItem(item);
                }
            }

            catch (Exception exception)
            {
                Debug.Fail(exception.ToString());
                Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
            }

            finally
            {
                ResumeNotification();
                OnAfterRemoveRange(new CollectionChangingEventArgs<T>(collection));
            }
        }

        #endregion

        #region ReplaceBy

        public void ReplaceBy(IEnumerable<T> collection)
        {
            bool hasChanged;
            ReplaceBy(collection, out hasChanged);
        }

        public void ReplaceBy(IEnumerable<T> collection, out bool hasChanged)
        {
            if (IsReadOnly) throw new InvalidOperationException("This collection is read only");
            ReplaceByItemRange(collection, true, out hasChanged);
        }

        #endregion

        #region ReplaceByItemRange

        protected virtual void ReplaceByItemRange(IEnumerable<T> collection, bool performChecks, out bool hasChanged)
        {
            #region PerformChecks

            if (performChecks)
            {
                if (collection == null) throw new ArgumentNullException();
                if (collection.Any(item => item == null)) throw new ArgumentException();
                if (!AllowDuplicates) collection = collection.Distinct();
            }

            #endregion

            if (!this.Any() && !collection.Any())
            {
                hasChanged = false;
                return;
            }

            if (!this.Any())
            {
                hasChanged = true;

                try
                {
                    SuspendNotification();
                    SuspensionRecords.Enqueue(new SuspensionRecord<T>(CollectionChangeType.Reset, null));
                    InsertItemRange(0, collection, performChecks);
                }

                catch (Exception exception)
                {
                    Debug.Fail(exception.ToString());
                    Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
                }

                finally
                {
                    ResumeNotification();
                }

                return;
            }

            if (!collection.Any())
            {
                hasChanged = true;

                try
                {
                    SuspendNotification();
                    SuspensionRecords.Enqueue(new SuspensionRecord<T>(CollectionChangeType.Reset, null));
                    RemoveItemRange(this, false);
                }

                catch (Exception exception)
                {
                    Debug.Fail(exception.ToString());
                    Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
                }

                finally
                {
                    ResumeNotification();
                }

                return;
            }

            if (this.SequenceEqual(collection))
            {
                hasChanged = false;
                return;
            }

            try
            {
                SuspendNotification();

                var itemsToInsert = collection.Except(_innerList);
                var itemsToRemove = _innerList.Except(collection);

                if (
                    itemsToRemove.Any() &&
                    !itemsToInsert.Any() &&
                    _innerList.Except(itemsToRemove).SequenceEqual(collection))
                {
                    // Typische Situation bei Anwenden eines Filters: Die Liste wird gegen eine Untermenge
                    // ihrer selbst ausgetauscht

                    RemoveItemRange(itemsToRemove, false);
                }

                else if (
                    itemsToInsert.Any() &&
                    !itemsToRemove.Any() &&
                    collection.Except(itemsToInsert).SequenceEqual(_innerList))
                {
                    if (_autoSort)
                    {
                        if (Comparer == null) collection = collection.OrderBy(item => item);
                        else collection = collection.OrderBy(item => item, Comparer);
                    }
                    else if (SourceCollection != null && SourceCollection.AutoSort)
                    {
                        if (SourceCollection.Comparer == null) collection = collection.OrderBy(item => item);
                        else collection = collection.OrderBy(item => item, SourceCollection.Comparer);
                    }

                    // Typische Situation beim Aufheben eines Filters: Die Liste wird gegen eine Übermenge
                    // ihrer selbst ausgetauscht

                    var i = 0;
                    foreach (var item in collection)
                    {
                        if (i == _innerList.Count || !_innerList[i].Equals(item)) InsertItem(i, item, performChecks);
                        i++;
                    }
                }

                else
                {
                    SuspensionRecords.Enqueue(new SuspensionRecord<T>(CollectionChangeType.Reset, null));
                    RemoveItemRange(this.ToList(), false);
                    InsertItemRange(0, collection, performChecks);
                }

                hasChanged = true;
            }

            catch (Exception exception)
            {
                hasChanged = false;
                Debug.Fail(exception.ToString());
                Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
            }

            finally
            {
                ResumeNotification(true);
            }
        }

        #endregion

        #region ResumeNotification

        public void ResumeNotification(bool raisePendingEvents = true, bool force = false)
        {
            if (force)
            {
                _notificationSuspendedCounter = 1;
            }
            _notificationSuspendedCounter = Math.Max(_notificationSuspendedCounter - 1, 0);
            if (IsNotificationSuspended) return;

            if (_suspensionRecords == null) return;

            if (!_suspensionRecords.Any() || !raisePendingEvents)
            {
                _suspensionRecords = null;
                return;
            }

            if (_suspensionRecords.Any(item => item.ChangeType == CollectionChangeType.Reset))
            {
                OnCollectionChanged(CollectionChangedEventArgs<T>.Reset());
            }
            else
            {
                foreach (var group in _suspensionRecords.GroupBy(record => record.ChangeType))
                {
                    switch (group.First().ChangeType)
                    {
                        case CollectionChangeType.Sorted:
                            OnCollectionChanged(CollectionChangedEventArgs<T>.Sorted());
                            break;

                        default:
                            {
                                OnCollectionChanged(new CollectionChangedEventArgs<T>(
                                group.First().ChangeType,
                                group.Select(item => item.Record)));
                            }
                            break;
                    }
                }
            }

            _suspensionRecords = null;
        }

        #endregion

        #region Shuffle

        public void Shuffle()
        {
            _innerList.Shuffle();
        }

        #endregion Shuffle

        #region Sort

        public void Sort()
        {
            Sort(Comparer);
        }

        public void Sort(IComparer<T> comparer)
        {
            OnBeforeSorted();

            if (comparer == null) _innerList.Sort();
            else _innerList.Sort(comparer);

            OnAfterSorted();

            if (IsNotificationSuspended)
            {
                SuspensionRecords.Enqueue(new SuspensionRecord<T>(CollectionChangeType.Sorted, null));
            }
            else
            {
                OnCollectionChanged(CollectionChangedEventArgs<T>.Sorted());
            }
        }

        public void Sort(Comparison<T> comparison)
        {
            _innerList.Sort(comparison);

            OnAfterSorted();

            if (IsNotificationSuspended)
            {
                SuspensionRecords.Enqueue(new SuspensionRecord<T>(CollectionChangeType.Sorted, null));
            }
            else
            {
                OnCollectionChanged(CollectionChangedEventArgs<T>.Sorted());
            }
        }

        #endregion Sort

        #region SuspendNotification

        int _notificationSuspendedCounter;

        public virtual void SuspendNotification()
        {
            _notificationSuspendedCounter++;
        }

        #endregion

        #region ToArray

        public T[] ToArray()
        {
            return _innerList.ToArray();
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return this.ToString("; ");
        }

        #endregion

        #endregion

        #region Ereignishandler

        #region Item_PropertyChanged

        bool _autoSorting;

        void item_PropertyChanged(T sender, PropertyChangedEventArgs<T> e)
        {
            /* This may happen in an exotic scenario: since delegates are multicast, another change
			 * event handler can remove the item for the collection before the collection gets
			 * the change event.
			 * We have this scenario with locations on the "reference" tab card: 
			 * 1) The user types a valid uri in the "online address" field -> a corresponding location is created.
			 * 2) The user changes the online address field to an invalid uri -> the main form event handler
			 *    removes the location from the smart repeater data source; at the same time, the smart repeater
			 *    data source receives the orginal change event. */

            // We use Contains and not IndexOf because Contains may be overriden with a much faster 
            // Dictionary implementation in large collections while IndexOf is relatively slow.
            if (!Contains(sender)) return;

            var generation = 1;
            if (e.CollectionChangedTrigger != null) CalculateGenerations(e.CollectionChangedTrigger, ref generation);
            if (generation <= _maxChangeNotificationGenerations)
            {
                var index = IndexOf(sender);

                if (IsNotificationSuspended)
                {
                    SuspensionRecords.Enqueue(new SuspensionRecord<T>(CollectionChangeType.ItemsChanged, new CollectionChangeRecord<T>(sender, index, e)));
                }
                else OnCollectionChanged(CollectionChangedEventArgs<T>.ItemChanged(sender, index, e));
            }

            if (Count > 1 && (_autoSort || (SourceCollection != null && SourceCollection.AutoSort)))
            {
                if (typeof(IResortAfterItemChange<T>).IsAssignableFrom(typeof(T)))
                {
                    if (((IResortAfterItemChange<T>)sender).SuppressResort(e)) return;
                }

                var oldIndex = IndexOf(sender);

                // BUG 14898: Identical references were resorted after an arbitray change
                if (Comparer == null)
                {
                    if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
                    {
                        var comparable = (IComparable<T>)sender;
                        if ((oldIndex == 0 || comparable.CompareTo(_innerList[oldIndex - 1]) >= 0) &&
                            (oldIndex == Count - 1 || comparable.CompareTo(_innerList[oldIndex + 1]) <= 0))
                        {
                            return;
                        }
                    }
                }
                else
                {
                    if ((oldIndex == 0 || Comparer.Compare(sender, _innerList[oldIndex - 1]) >= 0) &&
                        (oldIndex == Count - 1 || Comparer.Compare(sender, _innerList[oldIndex + 1]) <= 0))
                    {
                        return;
                    }
                }

                try
                {
                    _autoSorting = true;

                    RemoveFromInnerList(oldIndex, sender, true);

                    var newIndex = _innerList.BinarySearch(sender, Comparer);
                    if (newIndex < 0) newIndex = ~newIndex;

                    InsertToInnerList(newIndex, sender, true, e, oldIndex);
                }
                finally
                {
                    _autoSorting = false;
                }
            }
        }

        #endregion

        #region SourceCollection_CollectionChanged

        void sourceCollection_CollectionChanged(object sender, CollectionChangedEventArgs<T> e)
        {
            switch (e.ChangeType)
            {
                case CollectionChangeType.ItemsAdded:
                    {
                        if (Filter == null)
                        {
                            AddItemRange(from record in e.Records
                                         select record.Item, false);
                        }
                        else
                        {
                            /* This looks stupid: Checking for !Contains(record.Item)
                             * when SourceCollection.ChangeType is ItemsAdded
                             * However, it can occur under the following conditions:
                             * - A filtered collection is initialized from a source collection that contains "item"
                             * - The source collection's events are hold back (i.e. by SuspendNotification or 
                             *   a blocked SendEvent call from an async process.
                             * In these cases, the same item would be added twice.
                             * */
                            AddItemRange(from record in e.Records
                                         where
                                            Filter(record.Item) &&
                                            !Contains(record.Item)
                                         select record.Item, false);
                        }
                    }
                    break;

                case CollectionChangeType.ItemsChanged:
                    {
                        if (Filter != null)
                        {
                            RemoveItemRange(from record in e.Records
                                            where
                                                 !Filter(record.Item) &&
                                                 Contains(record.Item)
                                            select record.Item, false);

                            AddItemRange(from record in e.Records
                                         where
                                              Filter(record.Item) &&
                                              !Contains(record.Item)
                                         select record.Item, false);
                        }
                    }
                    break;

                case CollectionChangeType.ItemsDeleted:
                    {
                        RemoveItemRange(from record in e.Records
                                        select record.Item, false);
                    }
                    break;

                case CollectionChangeType.ItemsMoved:
                    {
                        if (SourceCollection != null && SourceCollection._autoSorting) return;

                        if (e.HasRecords)
                        {
                            foreach (var record in e.Records)
                            {
                                if (Contains(record.Item))
                                {
                                    var insertAfterItem = record.NewIndex == 0 ? default(T) : SourceCollection[record.NewIndex - 1];

                                    if (insertAfterItem != null && !Contains(insertAfterItem))
                                    {
                                        var found = false;
                                        for (int i = SourceCollection.IndexOf(insertAfterItem); i >= 0; i--)
                                        {
                                            if (Contains(SourceCollection[i]))
                                            {
                                                insertAfterItem = SourceCollection[i];
                                                found = true;
                                                break;
                                            }
                                        }

                                        if (!found) insertAfterItem = default(T);
                                    }

                                    Move(record.Item, insertAfterItem);
                                }
                            }
                        }
                    }
                    break;

                case CollectionChangeType.Reset:
                    {
                        Clear();

                        if (Filter == null)
                        {
                            AddItemRange(from record in e.Records
                                         select record.Item, false);
                        }
                        else
                        {
                            AddItemRange(from item in SourceCollection
                                         where Filter(item)
                                         select item, false);
                        }
                    }
                    break;

                case CollectionChangeType.Sorted:
                    // don't repeat, should be done on this collection directly
                    break;
            }
        }

        #endregion

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        #endregion

        #region IList<T> Members

        bool ICollection<T>.Remove(T item)
        {
            int index = IndexOf(item);
            if (index == -1) return false;

            Remove(item);
            return true;
        }

        #endregion

        #region IList Members

        int System.Collections.IList.Add(object value)
        {
            T item = (T)value;
            if (item == null) throw new ArgumentException();
            Add(item);
            return IndexOf(item);
        }

        void System.Collections.IList.Clear()
        {
            Clear();
        }

        bool System.Collections.IList.Contains(object value)
        {
            T item = (T)value;
            if (item == null) return false;
            return Contains(item);
        }

        int System.Collections.IList.IndexOf(object value)
        {
            T item = (T)value;
            if (item == null) return -1;
            return IndexOf(item);
        }

        void System.Collections.IList.Insert(int index, object value)
        {
            T item = (T)value;
            if (item == null) throw new ArgumentException();
            Insert(index, item);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        bool System.Collections.IList.IsFixedSize
        {
            get { return IsReadOnly; }
        }

        void System.Collections.IList.Remove(object value)
        {
            T item = (T)value;
            if (item == null) throw new ArgumentException();
            Remove(item);
        }

        object System.Collections.IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                T item = (T)value;
                if (item == null) throw new ArgumentException();
                this[index] = item;
            }
        }

        #endregion

        #region ICollection Members

        void System.Collections.ICollection.CopyTo(Array array, int index)
        {
            ((System.Collections.ICollection)_innerList).CopyTo(array, index);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        bool System.Collections.ICollection.IsSynchronized
        {
            get { return ((System.Collections.ICollection)_innerList).IsSynchronized; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        object System.Collections.ICollection.SyncRoot
        {
            get { return ((System.Collections.ICollection)_innerList).SyncRoot; }
        }

        #endregion
    }
}