using System.Text;

namespace SwissAcademic.Globalization
{
    public class BidiGroup
    {
        #region Konstruktoren

        internal BidiGroup(char c, BidiCategorySimplified bidiCategorySimplified)
        {
            BidiCategorySimplified = bidiCategorySimplified;
            _stringBuilder = new StringBuilder(c.ToString());
        }

        #endregion

        #region Eigenschaften

        #region BidiCategorySimplified

        public BidiCategorySimplified BidiCategorySimplified { get; private set; }

        #endregion

        #region Text

        internal StringBuilder _stringBuilder;

        public string Text
        {
            get { return _stringBuilder.ToString(); }
        }

        #endregion

        #endregion

        #region Methoden

        #region Append

        internal void Append(char c)
        {
            _stringBuilder.Append(c);
        }

        #endregion

        #endregion
    }
}
