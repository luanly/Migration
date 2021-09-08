using System.Drawing;

namespace SwissAcademic.Drawing
{
    public static class KnownColors
    {
        #region Konstruktoren

        static KnownColors()
        {
            AnnotationQuickReference100 = Color.FromArgb(255, 150, 150);
            AnnotationQuickReference50 = Color.FromArgb(255, 208, 208);

            AnnotationComment100 = Color.FromArgb(255, 204, 120);
            AnnotationComment50 = Color.FromArgb(255, 223, 170);

            AnnotationCommentBorder100 = Color.FromArgb(215, 188, 98);
            AnnotationCommentBorder50 = Color.FromArgb(242, 214, 124);

            AnnotationHighlightYellow100 = Color.FromArgb(255, 246, 0);
            AnnotationHighlightYellow50 = Color.FromArgb(255, 255, 180);

            AnnotationSummary100 = Color.FromArgb(180, 255, 180);
            AnnotationSummary50 = Color.FromArgb(220, 255, 220);

            AnnotationSummaryBorder100 = Color.FromArgb(128, 215, 128);
            AnnotationSummaryBorder50 = Color.FromArgb(162, 242, 162);

            AnnotationTaskItem100 = Color.FromArgb(147, 244, 244);
            AnnotationTaskItem50 = Color.FromArgb(197, 255, 255);

            AnnotationTaskItemBorder100 = Color.FromArgb(105, 206, 206);
            AnnotationTaskItemBorder50 = Color.FromArgb(133, 232, 232);

            AnnotationDirectQuotation100 = Color.FromArgb(160, 199, 255);
            AnnotationDirectQuotation50 = Color.FromArgb(214, 231, 255);


            AnnotationIndirectQuotation100 = Color.FromArgb(255, 196, 255);
            AnnotationIndirectQuotation50 = Color.FromArgb(255, 221, 255);

            AnnotationIndirectQuotationBorder100 = Color.FromArgb(215, 140, 215);
            AnnotationIndirectQuotationBorder50 = Color.FromArgb(242, 177, 242);

            AnnotationUnknownType100 = Color.FromArgb(211, 211, 211);
            AnnotationUnknownType50 = Color.FromArgb(231, 231, 231);

            AnnotationUnknownTypeBorder100 = Color.FromArgb(178, 178, 178);
            AnnotationUnknownTypeBorder50 = Color.FromArgb(200, 200, 200);

            AnnotationHighlightRed100 = Color.FromArgb(255, 150, 150);
            AnnotationHighlightRed50 = Color.FromArgb(255, 208, 208);

            ToolSelection = Color.FromArgb(175, 195, 215);
            SearchResultSelection = Color.FromArgb(175, 195, 215);

            PdfViewerBackground = Color.FromArgb(220, 220, 220);

            LineGray = Color.FromArgb(231, 227, 231);
            LightLineGray = Color.FromArgb(176, 180, 176);
            DarkLineGray = Color.FromArgb(176, 180, 176);
            DropDownBorder = Color.FromArgb(048, 104, 144);
            FontBlack = Color.FromArgb(75, 75, 75);
            FontGray = Color.FromArgb(128, 128, 128);
            HighlightYellow = Color.FromArgb(255, 255, 102);
            LinkBlue = Color.FromArgb(0, 102, 204);
            MarkupDuplicates = Color.FromArgb(249, 231, 157);
            MarkupStatusBarWarning = Color.FromArgb(249, 231, 157); //Color.FromArgb(255, 255, 128);
            MenuIconBackground = Color.FromArgb(192, 208, 232);
            MenuIconBorder = Color.FromArgb(048, 104, 144);
            MenuIconPressed = Color.FromArgb(144, 180, 224);
            SelectionGray = Color.FromArgb(190, 190, 190);
            TitleBlue = Color.FromArgb(21, 66, 139);
            TitleRed = Color.FromArgb(210, 0, 0);
            White = Color.FromArgb(255, 255, 255);

            LightGray = Color.FromArgb(242, 242, 242);

            LightBrown = LightGray;
            LightBlue = LightGray;
            LightRed = LightGray;
            LightGreen = LightGray;
            LightYellow = LightGray;

            //Identisch mit Dark-Farben
            Gray = Color.FromArgb(226, 226, 226);

            Brown = Gray;
            Blue = Gray;
            Red = Gray;
            Green = Gray;
            Yellow = Gray;

            DarkBrown = Gray;
            DarkBlue = Gray;
            DarkGray = Gray;
            DarkRed = Gray;
            DarkGreen = Gray;
            DarkYellow = Gray;


            DarkDarkBrown = Color.FromArgb(153, 128, 178);//OK C6
            DarkDarkBlue = Color.FromArgb(0, 84, 159);//OK C6
            DarkDarkGray = Color.FromArgb(100, 114, 140);//OK C6
            DarkDarkRed = Color.FromArgb(213, 43, 30);//OK C6
            DarkDarkGreen = Color.FromArgb(84, 172, 74); //OK C6
            DarkDarkYellow = Color.FromArgb(234, 128, 0); //OK C6


            //http://www.tech-archive.net/Archive/Word/microsoft.public.word.drawing.graphics/2007-12/msg00053.html
            WordHighlightYellow = Color.FromArgb(255, 255, 0);      //#FFFF00 255/255/000 Yellow*
            WordHighlightRed = Color.FromArgb(255, 0, 0);           //#FF0000 255/000/000 Red*
            WordHighlightDarkRed = Color.FromArgb(139, 0, 0);       //#8B0000 139/000/000 Dark Red*

            WordHighlightBrightGreen = Color.FromArgb(0, 255, 0);   //#00FF00 000/255/000 Bright Green(Lime)*
            WordHighlightDarkBlue = Color.FromArgb(0, 0, 139);      //#00008B 000/000/139 Dark Blue(Navy)*
            WordHighlightDarkYellow = Color.FromArgb(128, 128, 0);  //#808000 128/128/000 Dark Yellow(Olive)*

            WordHighlightTurquoise = Color.FromArgb(0, 255, 255);   //#00FFFF 000/255/255 Turquoise(Aqua)*
            WordHighlightTeal = Color.FromArgb(0, 128, 128);        //#008080 000/128/128 Teal*
            WordHighlightGray = Color.FromArgb(128, 128, 128);      //808080 128/128/128 Gray-50%(Gray)*

            WordHighlightPink = Color.FromArgb(255, 0, 255);        //#FF00FF 255/000/255 Pink(Fuschia)*
            WordHighlightGreen = Color.FromArgb(0, 128, 0);         //#008000 000/128/000 Green*
            WordHighlightSilver = Color.FromArgb(192, 192, 192);    //#C0C0C0 192/192/192 Gray-25%(Silver)*

            WordHighlightBlue = Color.FromArgb(0, 0, 255);          //#0000FF 000/000/255 Blue*
            WordHighlightViolet = Color.FromArgb(128, 0, 128);      //#800080 128/000/128 Violet(Purple)*
            WordHighlightBlack = Color.FromArgb(255, 255, 255);		//#FFFFFF 255/255/255 Black
        }

        #endregion

        #region Eigenschaften

        public static Color AnnotationDirectQuotation100 { get; private set; }
        public static Color AnnotationDirectQuotation50 { get; private set; }

        public static Color AnnotationHighlightRed100 { get; private set; }
        public static Color AnnotationHighlightRed50 { get; private set; }

        public static Color AnnotationHighlightYellow100 { get; private set; }
        public static Color AnnotationHighlightYellow50 { get; private set; }

        public static Color AnnotationIndirectQuotation100 { get; private set; }
        public static Color AnnotationIndirectQuotation50 { get; private set; }

        public static Color AnnotationIndirectQuotationBorder100 { get; private set; }
        public static Color AnnotationIndirectQuotationBorder50 { get; private set; }

        public static Color AnnotationSummary100 { get; private set; }
        public static Color AnnotationSummary50 { get; private set; }

        public static Color AnnotationSummaryBorder100 { get; private set; }
        public static Color AnnotationSummaryBorder50 { get; private set; }

        public static Color AnnotationComment100 { get; private set; }
        public static Color AnnotationComment50 { get; private set; }

        public static Color AnnotationCommentBorder100 { get; private set; }
        public static Color AnnotationCommentBorder50 { get; private set; }

        public static Color AnnotationQuickReference100 { get; private set; }
        public static Color AnnotationQuickReference50 { get; private set; }

        public static Color AnnotationTaskItem100 { get; private set; }
        public static Color AnnotationTaskItem50 { get; private set; }

        public static Color AnnotationTaskItemBorder100 { get; private set; }
        public static Color AnnotationTaskItemBorder50 { get; private set; }

        public static Color AnnotationUnknownType100 { get; private set; }
        public static Color AnnotationUnknownType50 { get; private set; }

        public static Color AnnotationUnknownTypeBorder100 { get; private set; }
        public static Color AnnotationUnknownTypeBorder50 { get; private set; }

        public static Color ChatColorBlue { get; } = Color.FromArgb(0, 84, 159);
        public static Color ChatColorGreen { get; } = Color.FromArgb(84, 172, 74);
        public static Color ChatColorPurple { get; } = Color.FromArgb(117, 0, 159);
        public static Color ChatColorBrown { get; } = Color.FromArgb(159, 58, 0);
        public static Color ChatColorYellow { get; } = Color.FromArgb(159, 138, 0);
        public static Color ChatColorRed { get; } = Color.FromArgb(243, 43, 13);
        public static Color ChatColorDarkRed { get; } = Color.FromArgb(172, 74, 84);
        public static Color ChatColorDarkBrown { get; } = Color.FromArgb(172, 118, 74);
        public static Color ChatColorOrange { get; } = Color.FromArgb(242, 115, 47);
        public static Color ChatColorDarkRed2 { get; } = Color.FromArgb(159, 31, 65);

        public static Color ToolSelection { get; private set; }
        public static Color SearchResultSelection { get; private set; }

        public static Color PdfViewerBackground { get; private set; }

        public static Color LineGray { get; private set; }
        public static Color LightLineGray { get; private set; }
        public static Color DarkLineGray { get; private set; }
        public static Color DropDownBorder { get; private set; }
        public static Color FontBlack { get; private set; }
        public static Color FontGray { get; private set; }
        public static Color HighlightYellow { get; private set; }
        public static Color LinkBlue { get; private set; }
        public static Color MarkupDuplicates { get; private set; }
        public static Color MarkupStatusBarWarning { get; private set; }
        public static Color MenuIconBackground { get; private set; }
        public static Color MenuIconBorder { get; private set; }
        public static Color MenuIconPressed { get; private set; }
        public static Color SelectionGray { get; private set; }

        public static Color TitleBlue { get; private set; }
        public static Color TitleRed { get; private set; }
        public static Color White { get; private set; }

        public static Color LightBrown { get; private set; }
        public static Color LightBlue { get; private set; }
        public static Color LightGray { get; private set; }
        public static Color LightRed { get; private set; }
        public static Color LightGreen { get; private set; }
        public static Color LightYellow { get; private set; }

        public static Color Brown { get; private set; }
        public static Color Blue { get; private set; }
        public static Color Gray { get; private set; }
        public static Color Red { get; private set; }
        public static Color Green { get; private set; }
        public static Color Yellow { get; private set; }

        public static Color DarkBrown { get; private set; }
        public static Color DarkBlue { get; private set; }
        public static Color DarkGray { get; private set; }
        public static Color DarkRed { get; private set; }
        public static Color DarkGreen { get; private set; }
        public static Color DarkYellow { get; private set; }

        public static Color DarkDarkBrown { get; private set; }
        public static Color DarkDarkBlue { get; private set; }
        public static Color DarkDarkGray { get; private set; }
        public static Color DarkDarkRed { get; private set; }
        public static Color DarkDarkGreen { get; private set; }
        public static Color DarkDarkYellow { get; private set; }

        public static Color WordHighlightYellow { get; private set; }
        public static Color WordHighlightRed { get; private set; }
        public static Color WordHighlightDarkRed { get; private set; }

        public static Color WordHighlightBrightGreen { get; private set; }
        public static Color WordHighlightDarkBlue { get; private set; }
        public static Color WordHighlightDarkYellow { get; private set; }

        public static Color WordHighlightTurquoise { get; private set; }
        public static Color WordHighlightTeal { get; private set; }
        public static Color WordHighlightGray { get; private set; }

        public static Color WordHighlightPink { get; private set; }
        public static Color WordHighlightGreen { get; private set; }
        public static Color WordHighlightSilver { get; private set; }

        public static Color WordHighlightBlue { get; private set; }
        public static Color WordHighlightViolet { get; private set; }
        public static Color WordHighlightBlack { get; private set; }

        #region ProjectorMode

        static bool _projectorMode;

        public static bool ProjectorMode
        {
            get { return _projectorMode; }

            set
            {
                if (value == _projectorMode) return;
                _projectorMode = value;

                if (value)
                {
                    LightLineGray = Color.Black;
                    LineGray = Color.Black;
                    DarkLineGray = Color.Black;
                    DropDownBorder = Color.Black;
                    FontBlack = Color.Black;
                }

                else
                {
                    LightLineGray = Color.FromArgb(176, 180, 176);
                    LineGray = Color.FromArgb(231, 227, 231);
                    DarkLineGray = Color.FromArgb(176, 180, 176);
                    DropDownBorder = Color.FromArgb(048, 104, 144);
                    FontBlack = Color.FromArgb(75, 75, 75);
                }

                KnownBrushes.Reset();
                KnownPens.Reset();
            }
        }

        #endregion

        #endregion

        #region Methoden

        #region GetColor

        public static Color GetColor(KnownColorIdentifier identifier)
        {
            switch (identifier)
            {
                case KnownColorIdentifier.Brown:
                    return Brown;

                case KnownColorIdentifier.Blue:
                    return Blue;

                case KnownColorIdentifier.LineGray:
                    return LineGray;

                case KnownColorIdentifier.DarkBrown:
                    return Brown;

                case KnownColorIdentifier.DarkBlue:
                    return DarkBlue;

                case KnownColorIdentifier.DarkDarkBrown:
                    return DarkDarkBrown;

                case KnownColorIdentifier.DarkDarkBlue:
                    return DarkDarkBlue;

                case KnownColorIdentifier.DarkDarkGray:
                    return DarkDarkGray;

                case KnownColorIdentifier.DarkDarkRed:
                    return DarkDarkRed;

                case KnownColorIdentifier.DarkDarkGreen:
                    return DarkDarkGreen;

                case KnownColorIdentifier.DarkDarkYellow:
                    return DarkDarkYellow;

                case KnownColorIdentifier.DarkGray:
                    return DarkGray;

                case KnownColorIdentifier.DarkLineGray:
                    return DarkLineGray;

                case KnownColorIdentifier.DarkRed:
                    return DarkRed;

                case KnownColorIdentifier.DarkGreen:
                    return DarkGreen;

                case KnownColorIdentifier.DarkYellow:
                    return DarkYellow;

                case KnownColorIdentifier.FontBlack:
                    return FontBlack;

                case KnownColorIdentifier.FontGray:
                    return FontGray;

                case KnownColorIdentifier.Gray:
                    return Gray;

                case KnownColorIdentifier.HighlightYellow:
                    return HighlightYellow;

                case KnownColorIdentifier.LightBrown:
                    return LightBrown;

                case KnownColorIdentifier.LightBlue:
                    return LightBlue;

                case KnownColorIdentifier.LightGray:
                    return LightGray;

                case KnownColorIdentifier.LightRed:
                    return LightRed;

                case KnownColorIdentifier.LightGreen:
                    return LightGreen;

                case KnownColorIdentifier.LightYellow:
                    return LightYellow;

                case KnownColorIdentifier.LightLineGray:
                    return LightLineGray;

                case KnownColorIdentifier.LinkBlue:
                    return LinkBlue;

                case KnownColorIdentifier.MarkupDuplicates:
                    return MarkupDuplicates;

                case KnownColorIdentifier.MarkupStatusBarWarning:
                    return MarkupStatusBarWarning;

                case KnownColorIdentifier.MenuIconBackground:
                    return MenuIconBackground;

                case KnownColorIdentifier.MenuIconBorder:
                    return MenuIconBorder;

                case KnownColorIdentifier.MenuIconPressed:
                    return MenuIconPressed;

                case KnownColorIdentifier.Red:
                    return Red;

                case KnownColorIdentifier.Green:
                    return Green;

                case KnownColorIdentifier.SelectionGray:
                    return SelectionGray;

                case KnownColorIdentifier.TitleBlue:
                    return TitleBlue;

                case KnownColorIdentifier.TitleRed:
                    return TitleRed;

                case KnownColorIdentifier.White:
                    return White;

                case KnownColorIdentifier.Yellow:
                    return Yellow;

                default:
                    return White;
            }
        }

        #endregion

        #endregion
    }
}
