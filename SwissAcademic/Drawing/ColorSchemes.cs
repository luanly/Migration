
namespace SwissAcademic.Drawing
{
    public static class ColorSchemes
    {
        #region Konstruktoren

        static ColorSchemes()
        {
            Brown = new ColorScheme(ColorSchemeIdentifier.Brown, KnownColors.LightBrown, KnownColors.Brown, KnownColors.DarkBrown, KnownColors.DarkDarkBrown);
            Blue = new ColorScheme(ColorSchemeIdentifier.Blue, KnownColors.LightBlue, KnownColors.Blue, KnownColors.DarkBlue, KnownColors.DarkDarkBlue);
            Gray = new ColorScheme(ColorSchemeIdentifier.Gray, KnownColors.LightGray, KnownColors.Gray, KnownColors.DarkGray, KnownColors.DarkDarkGray);
            Red = new ColorScheme(ColorSchemeIdentifier.Red, KnownColors.LightRed, KnownColors.Red, KnownColors.DarkRed, KnownColors.DarkDarkRed);
            Green = new ColorScheme(ColorSchemeIdentifier.Green, KnownColors.LightGreen, KnownColors.Green, KnownColors.DarkGreen, KnownColors.DarkDarkGreen);
            Yellow = new ColorScheme(ColorSchemeIdentifier.Yellow, KnownColors.LightYellow, KnownColors.Yellow, KnownColors.DarkYellow, KnownColors.DarkDarkYellow);
        }

        #endregion

        #region Eigenschaften

        public static ColorScheme Brown { get; private set; }
        public static ColorScheme Blue { get; private set; }
        public static ColorScheme Gray { get; private set; }
        public static ColorScheme Red { get; private set; }
        public static ColorScheme Green { get; private set; }
        public static ColorScheme Yellow { get; private set; }

        public static ColorScheme Default
        {
            get { return Blue; }
        }

        #endregion

        #region Methoden

        #region GetColorScheme

        public static ColorScheme GetColorScheme(ColorSchemeIdentifier identifier)
        {
            switch (identifier)
            {
                case ColorSchemeIdentifier.Brown:
                    return Brown;

                case ColorSchemeIdentifier.Gray:
                    return Gray;

                case ColorSchemeIdentifier.Red:
                    return Red;

                case ColorSchemeIdentifier.Green:
                    return Green;

                case ColorSchemeIdentifier.Yellow:
                    return Yellow;

                default:
                    return Blue;
            }
        }

        #endregion

        #endregion
    }
}
