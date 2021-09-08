using System.Collections.Generic;

namespace SwissAcademic.Collections
{
    public class Group<T>
    {
        #region Konstruktoren

        public Group(GroupDescriptor<T> groupDescriptor, object header)
        {
            GroupDescriptor = groupDescriptor;
            Header = header;
            Items = new List<T>();
        }

        #endregion

        #region Eigenschaften

        #region GroupDescriptor

        public GroupDescriptor<T> GroupDescriptor { get; private set; }

        #endregion

        #region HasSubgroups

        public bool HasSubgroups
        {
            get { return Subgroups != null; }
        }

        #endregion

        #region Header

        public object Header { get; private set; }

        #endregion

        #region HeaderText

        public string HeaderText
        {
            get
            {
                if (Header == null) return SwissAcademic.Resources.Strings.NoInformation;
                var text = Header.ToString();
                //z.B. PageCount. Da ist Header nicht null, ToString() rufe aber PrettyString auf. Dies kann Null sein!
                if (string.IsNullOrEmpty(text)) return SwissAcademic.Resources.Strings.NoInformation;
                return text;
            }
        }

        #endregion

        #region Items

        public List<T> Items { get; private set; }

        #endregion

        #region Level

        public int Level
        {
            get
            {
                if (Parent == null) return 0;
                return Parent.Level + 1;
            }
        }

        #endregion

        #region Parent

        public Group<T> Parent { get; internal set; }

        #endregion

        #region Subgroups

        public IEnumerable<Group<T>> Subgroups { get; internal set; }

        #endregion

        #endregion
    }
}
