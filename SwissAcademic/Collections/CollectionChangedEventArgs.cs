using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SwissAcademic.Collections
{
    public delegate void CollectionChangedEventHandler<T>(object sender, CollectionChangedEventArgs<T> e);
    public delegate void CollectionChangingEventHandler<T>(object sender, CollectionChangingEventArgs<T> e);

    #region CollectionChangeType

    public enum CollectionChangeType
    {
        ItemsChanged,
        ItemsAdded,
        ItemsDeleted,
        ItemsMoved,
        Reset,
        Sorted
    }

    #endregion

    #region CollectionChangeRecord

    [DataContract]
    public class CollectionChangeRecord<T>
        :
        ICollectionChangeRecord
    {
        #region Konstruktoren

        public CollectionChangeRecord(T item, int index, PropertyChangedEventArgs<T> propertyChangedEventArgs)
            :
            this(item, index, -1, propertyChangedEventArgs)
        { }

        public CollectionChangeRecord(T item, int newIndex)
            :
            this(item, newIndex, -1, null)
        { }

        public CollectionChangeRecord(T item, int newIndex, int oldIndex)
            :
            this(item, newIndex, oldIndex, null)
        { }

        [JsonConstructor]
        public CollectionChangeRecord(T item, int newIndex, int oldIndex, PropertyChangedEventArgs<T> trigger)
        {
            if (item == null) throw new ArgumentNullException();

            Item = item;
            NewIndex = newIndex;
            OldIndex = oldIndex;
            Trigger = trigger;
        }

        #endregion

        #region Eigenschaften

        #region Item

        [DataMember]
        public T Item { get; private set; }

        #endregion

        #region NewIndex

        [DataMember]
        public int NewIndex { get; private set; }

        #endregion

        #region PropertyChangedEventArgs

        [DataMember]
        public PropertyChangedEventArgs<T> Trigger { get; private set; }

        #endregion

        #region OldIndex

        [DataMember]
        public int OldIndex { get; private set; }

        #endregion

        #endregion

        #region ICollectionChangeRecord Members

        object ICollectionChangeRecord.Item
        {
            get { return Item; }
        }

        IPropertyChangedEventArgs ICollectionChangeRecord.Trigger
        {
            get { return Trigger; }
        }

        #endregion
    }

    #endregion

    #region CollectionChangedEventArgs

    [DataContract]
    public class CollectionChangedEventArgs<T>
        :
        ICollectionChangedEventArgs
    {
        #region Konstruktoren

        [JsonConstructor]
        public CollectionChangedEventArgs(CollectionChangeType changeType, IEnumerable<CollectionChangeRecord<T>> records)
        {
            ChangeType = changeType;

            //Wir müssen hier ToList() aufrufen, da IEnumerable<T> nicht serialisierbar ist (z.B. Word-AddIn)
            Records = records == null ? new List<CollectionChangeRecord<T>>() : records.ToList();
        }

        #endregion

        #region Eigenschaften

        #region ChangeType

        [DataMember]
        public CollectionChangeType ChangeType { get; private set; }

        #endregion

        #region FirstRecordTriggerProperty

        public PropertyDescriptor<T> FirstRecordTriggerProperty
        {
            get
            {
                if (!HasRecords || Records.First().Trigger == null) return null;
                return Records.First().Trigger.Property;
            }
        }

        #endregion

        #region HasRecords

        public bool HasRecords
        {
            get { return Records != null && Records.Any(); }
        }

        #endregion

        #region Records


        [DataMember]
        public List<CollectionChangeRecord<T>> Records { get; }

        #endregion

        #endregion

        #region Methoden

        #region ToString

        public override string ToString()
        {
            if (FirstRecordTriggerProperty == null)
            {
                return string.Format("{0}, {1} records",
                    ChangeType,
                    HasRecords ? Records.Count() : 0);
            }
            else
            {
                return string.Format("{0}, {1} records (triggered by {2})",
                    ChangeType,
                    HasRecords ? Records.Count() : 0,
                    FirstRecordTriggerProperty);
            }
        }

        #endregion

        #endregion

        #region Statische Konstruktoren

        #region ItemChanged

        public static CollectionChangedEventArgs<T> ItemChanged(T item, int index, PropertyChangedEventArgs<T> trigger)
        {
            return new CollectionChangedEventArgs<T>(
                CollectionChangeType.ItemsChanged, new List<CollectionChangeRecord<T>>(1) { new CollectionChangeRecord<T>(item, index, trigger) });
        }

        #endregion

        #region ItemAdded

        public static CollectionChangedEventArgs<T> ItemAdded(T item, int newIndex)
        {
            return new CollectionChangedEventArgs<T>(
                CollectionChangeType.ItemsAdded, new List<CollectionChangeRecord<T>>(1)
                { new CollectionChangeRecord<T>(item, newIndex) });
        }

        #endregion

        #region ItemsAdded

        public static CollectionChangedEventArgs<T> ItemsAdded(List<CollectionChangeRecord<T>> records)
        {
            return new CollectionChangedEventArgs<T>(CollectionChangeType.ItemsAdded, records);
        }

        #endregion

        #region ItemDeleted

        public static CollectionChangedEventArgs<T> ItemDeleted(T item, int newIndex)
        {
            return new CollectionChangedEventArgs<T>(
                CollectionChangeType.ItemsDeleted, new List<CollectionChangeRecord<T>>(1) { new CollectionChangeRecord<T>(item, newIndex, -1) });
        }

        #endregion

        #region ItemsDeleted

        public static CollectionChangedEventArgs<T> ItemsDeleted(List<CollectionChangeRecord<T>> records)
        {
            return new CollectionChangedEventArgs<T>(CollectionChangeType.ItemsDeleted, records);
        }

        #endregion

        #region ItemMoved

        public static CollectionChangedEventArgs<T> ItemMoved(T item, int newIndex, int oldIndex, PropertyChangedEventArgs<T> trigger)
        {
            return new CollectionChangedEventArgs<T>(
                CollectionChangeType.ItemsMoved, new List<CollectionChangeRecord<T>>(1) { new CollectionChangeRecord<T>(item, newIndex, oldIndex, trigger) });
        }

        #endregion

        #region ItemsMoved

        public static CollectionChangedEventArgs<T> ItemsMoved(List<CollectionChangeRecord<T>> records)
        {
            return new CollectionChangedEventArgs<T>(CollectionChangeType.ItemsMoved, records);
        }

        #endregion

        #region Reset

        public static CollectionChangedEventArgs<T> Reset()
        {
            return new CollectionChangedEventArgs<T>(CollectionChangeType.Reset, null);
        }

        #endregion

        #region Sorted

        public static CollectionChangedEventArgs<T> Sorted()
        {
            return new CollectionChangedEventArgs<T>(CollectionChangeType.Sorted, null);
        }

        #endregion

        #endregion

        #region ICollectionChangedEventArgs Members

        IEnumerable<ICollectionChangeRecord> ICollectionChangedEventArgs.Records
        {
            get { return Records.Cast<ICollectionChangeRecord>(); }
        }

        #endregion
    }

    #endregion

    #region CollectionChangingEventArgs

    public class CollectionChangingEventArgs<T>
        :
        EventArgs
    {
        #region Konstruktoren

        public CollectionChangingEventArgs(IEnumerable<T> collection)
        {
            Collection = collection;
        }

        #endregion

        #region Collection

        public IEnumerable<T> Collection { get; private set; }

        #endregion
    }

    #endregion
}
