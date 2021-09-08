using System;
using System.Drawing;

namespace SwissAcademic.Drawing
{
    [Serializable]
    public class ColorScheme
    {
        #region Konstruktoren

        public ColorScheme(ColorSchemeIdentifier identifier, Color light, Color middle, Color dark, Color darkdark)
        {
            Identifier = identifier;
            Light = light;
            Middle = middle;
            Dark = dark;
            DarkDark = darkdark;
            ReadOnly = Color.FromArgb(240, 240, 240);
        }

        #endregion

        #region Eigenschaften

        public ColorSchemeIdentifier Identifier { get; private set; }

#if DEBUG
        public Color Light { get; set; }
        public Color Middle { get; set; }
        public Color Dark { get; set; }
        public Color DarkDark { get; set; }
        public Color ReadOnly { get; set; }
#else
		public Color Light { get; private set; }
		public Color Middle { get; private set; }
		public Color Dark { get; private set; }
		public Color DarkDark { get; private set; }
		public Color ReadOnly { get; private set; }
#endif

        #endregion

        #region Methoden

        #region GetKnownColorIdentifier

        public KnownColorIdentifier GetKnownColorIdentifier(ColorSchemeMember member)
        {
            switch (member)
            {
                case ColorSchemeMember.Light:
                    {
                        switch (Identifier)
                        {
                            case ColorSchemeIdentifier.Brown:
                                return KnownColorIdentifier.LightBrown;

                            case ColorSchemeIdentifier.Gray:
                                return KnownColorIdentifier.LightGray;

                            case ColorSchemeIdentifier.Red:
                                return KnownColorIdentifier.LightRed;

                            case ColorSchemeIdentifier.Green:
                                return KnownColorIdentifier.LightGreen;

                            case ColorSchemeIdentifier.Yellow:
                                return KnownColorIdentifier.LightYellow;

                            default:
                                return KnownColorIdentifier.LightBlue;
                        }
                    }

                case ColorSchemeMember.Dark:
                    {
                        switch (Identifier)
                        {
                            case ColorSchemeIdentifier.Brown:
                                return KnownColorIdentifier.DarkBrown;

                            case ColorSchemeIdentifier.Gray:
                                return KnownColorIdentifier.DarkGray;

                            case ColorSchemeIdentifier.Red:
                                return KnownColorIdentifier.DarkRed;

                            case ColorSchemeIdentifier.Green:
                                return KnownColorIdentifier.DarkGreen;

                            case ColorSchemeIdentifier.Yellow:
                                return KnownColorIdentifier.DarkYellow;

                            default:
                                return KnownColorIdentifier.DarkBlue;
                        }
                    }

                case ColorSchemeMember.DarkDark:
                    {
                        switch (Identifier)
                        {
                            case ColorSchemeIdentifier.Brown:
                                return KnownColorIdentifier.DarkDarkBrown;

                            case ColorSchemeIdentifier.Gray:
                                return KnownColorIdentifier.DarkDarkGray;

                            case ColorSchemeIdentifier.Red:
                                return KnownColorIdentifier.DarkDarkRed;

                            case ColorSchemeIdentifier.Green:
                                return KnownColorIdentifier.DarkDarkGreen;

                            case ColorSchemeIdentifier.Yellow:
                                return KnownColorIdentifier.DarkDarkYellow;

                            default:
                                return KnownColorIdentifier.DarkDarkBlue;
                        }
                    }

                default:
                    {
                        switch (Identifier)
                        {
                            case ColorSchemeIdentifier.Brown:
                                return KnownColorIdentifier.Brown;

                            case ColorSchemeIdentifier.Gray:
                                return KnownColorIdentifier.Gray;

                            case ColorSchemeIdentifier.Red:
                                return KnownColorIdentifier.Red;

                            case ColorSchemeIdentifier.Green:
                                return KnownColorIdentifier.Green;

                            case ColorSchemeIdentifier.Yellow:
                                return KnownColorIdentifier.Yellow;

                            default:
                                return KnownColorIdentifier.Blue;
                        }
                    }
            }
        }

        #endregion

        #region GetMember

        public Color GetMember(ColorSchemeMember member)
        {
            switch (member)
            {
                case ColorSchemeMember.Light:
                    return Light;

                case ColorSchemeMember.Dark:
                    return Dark;

                case ColorSchemeMember.DarkDark:
                    return DarkDark;

                default:
                    return Middle;
            }
        }

        #endregion

        #endregion
    }
}
