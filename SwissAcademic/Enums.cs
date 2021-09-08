using System;
using System.Text;

namespace System
{
    #region BracketMatching

    public enum BracketMatching
    {
        Undefined,
        Paired,
        LeftBracketOnly,
        RightBracketOnly
    }

    #endregion

    #region IllegalCharacters

    [FlagsAttribute]
    public enum IllegalCharacters
    {
        None,
        Return = 1,
        SemiColon = 2,
        Slash = 4,
        Tab = 8,
        Backslash = 16,
        ZeroWidthBreakOpportunity = 32,
        NonStandardWhitespace = 64,
    }

    #endregion IllegalCharacters
}

namespace SwissAcademic
{
    #region MeasurementUnitType

    public enum MeasurementUnitType
    {
        Centimeters,
        Inches,
        Millimeters,
        Numeric,
        Points,
        Twips
    }

    #endregion MeasurementUnitType

    #region BackupWorkerErrorCodes

    public enum BackupWorkerErrorCodes
    {
        Success = 0,
        ProjectFileNotFound = 1,
        ProjectFileOpenFailed = 2,
        BackupPathNotFound = 3,
        BackupPathAccessDenied = 4,
        BackupWriteFailed = 5,
        Cancel = 6,
        Unknown = 7
    }

    #endregion BackupWorkerErrorCodes

    #region BuildType

    public enum BuildType
    {
        Alpha = 0,
        Beta = 1,
        Release = 2,
    }

    #endregion
}

namespace SwissAcademic.Controls
{
    #region DropDownStyle


    /// <summary>
    /// Specifies the style of the dropdown part of a text editor control.
    /// </summary>
    /// <value></value>

    public enum DropDownStyle
    {
        /// <summary>
        /// No dropdown box will be displayed.
        /// </summary>
        None,
        Popup,
        Custom,
        CustomPopup,
        CustomList,
        /// <summary>
        /// A list will be displayed temporarily.
        /// </summary>
        DropDown,
        /// <summary>
        /// A list will be displayed permanently.
        /// </summary>
        DropDownList
    }

    #endregion
}

namespace SwissAcademic.Drawing
{
    #region ColorSchemeIdentifier

    public enum ColorSchemeIdentifier
    {
        Brown = 0,
        Blue = 1,
        Gray = 2,
        Red = 3,
        Green = 4,
        Yellow = 5
    }

    #endregion ColorSchemeIdentifier

    #region ColorSchemeMember

    public enum ColorSchemeMember
    {
        None,
        Light,
        Middle,
        Dark,
        DarkDark
    }

    #endregion ColorSchemeMember

    #region FontStyle

    [Flags]
    public enum FontStyle
    {
        //JHP Idea (very wild one): Transform into struct or class and overload bitwise operators, so that it becomes correct to bitwise AND e.g. SameAsPrevious & Italic
        //http://stackoverflow.com/questions/1355817/how-do-i-overload-an-operator-for-an-enumeration-in-c
        //See usage of TaggedTextToTextUnits.TaggedTextToTextUnits
        //Others have had the same issue too:
        //http://stackoverflow.com/questions/5537078/negative-flags-in-c-sharp

        //TODO JHP: Make SameAsPrevious and SameAsNext combinable with other values, ONLY FOR RUNS inside TaggedText
        //So that the formatting inside the TaggedText field temporarily switches on some additional formatting while keeping the 
        //SameAsPrevious as the baseline and not disrupting to pass the SameAsPrevious info on to the following component part. 

        SameAsPrevious = 524288,//-2,
        SameAsNext = 262144,//- 1,
        Neutral = 0,

        // Kompatibel zu den Werten von Subsystems RTF Generator
        AllCaps = 65536,
        Bold = 2,
        DoubleUnderline = 256,
        Italic = 4,
        SmallCaps = 131072,
        StrikeThrough = 8,
        Subscript = 32,
        Superscript = 16,
        Underline = 1
    }

    #endregion FontStyle

    #region KnownColorIdentifier

    public enum KnownColorIdentifier
    {
        LineGray,
        LightLineGray,
        DarkLineGray,
        FontBlack,
        FontGray,
        HighlightYellow,
        LinkBlue,
        MarkupDuplicates,
        MarkupStatusBarWarning,
        MenuIconBackground,
        MenuIconBorder,
        MenuIconPressed,
        SelectionGray,
        TitleBlue,
        TitleRed,
        White,

        LightBrown,
        LightBlue,
        LightGray,
        LightRed,
        LightGreen,
        LightYellow,

        Brown,
        Blue,
        Gray,
        Red,
        Green,
        Yellow,

        DarkBrown,
        DarkBlue,
        DarkGray,
        DarkRed,
        DarkGreen,
        DarkYellow,

        DarkDarkBrown,
        DarkDarkBlue,
        DarkDarkGray,
        DarkDarkRed,
        DarkDarkGreen,
        DarkDarkYellow,
    }

    #endregion KnownColorIdentifier

    #region SimpleFontStyle

    public class SimpleFontStyle
    {
        #region Konstruktoren

        public SimpleFontStyle()
        {
            Formatting = SimpleFontStyleFormatting.None;
            Caps = SimpleFontStyleCaps.None;
            Script = SimpleFontStyleScript.None;
        }

        public SimpleFontStyle(SimpleFontStyleFormatting formatting, SimpleFontStyleCaps caps, SimpleFontStyleScript script)
        {
            Formatting = formatting;
            Caps = caps;
            Script = script;
        }

        #endregion

        #region Eigenschaften

        #region Caps

        public SimpleFontStyleCaps Caps { get; set; }

        #endregion

        #region Formatting

        public SimpleFontStyleFormatting Formatting { get; set; }

        #endregion

        #region Script

        public SimpleFontStyleScript Script { get; set; }

        #endregion

        #endregion

        #region Methoden

        #region ToString

        public override string ToString()
        {
            if (Formatting == SimpleFontStyleFormatting.Unspecified)
            {
                return SwissAcademic.Resources.ResourceHelper.GetEnumName(Formatting);
            }

            else
            {
                StringBuilder stringBuilder = new StringBuilder();

                switch (Caps)
                {
                    case SimpleFontStyleCaps.AllCaps:
                    case SimpleFontStyleCaps.SmallCaps:
                        stringBuilder.Append(", " + SwissAcademic.Resources.ResourceHelper.GetEnumName(Caps));
                        break;
                }

                switch (Script)
                {
                    case SimpleFontStyleScript.Subscript:
                    case SimpleFontStyleScript.Superscript:
                        stringBuilder.Append(", " + SwissAcademic.Resources.ResourceHelper.GetEnumName(Script));
                        break;
                }

                if ((Formatting & SimpleFontStyleFormatting.Italic) == SimpleFontStyleFormatting.Italic)
                {
                    stringBuilder.Append(", " + SwissAcademic.Resources.ResourceHelper.GetEnumName(SimpleFontStyleFormatting.Italic));
                }

                if ((Formatting & SimpleFontStyleFormatting.Bold) == SimpleFontStyleFormatting.Bold)
                {
                    stringBuilder.Append(", " + SwissAcademic.Resources.ResourceHelper.GetEnumName(SimpleFontStyleFormatting.Bold));
                }

                if ((Formatting & SimpleFontStyleFormatting.Underlined) == SimpleFontStyleFormatting.Underlined)
                {
                    stringBuilder.Append(", " + SwissAcademic.Resources.ResourceHelper.GetEnumName(SimpleFontStyleFormatting.Underlined));
                }

                if (stringBuilder.Length == 0)
                {
                    return SwissAcademic.Resources.ResourceHelper.GetEnumName(SimpleFontStyleFormatting.None);
                }
                else
                {
                    return stringBuilder.ToString(2, stringBuilder.Length - 2);
                }
            }
        }

        #endregion

        #endregion
    }

    #endregion SimpleFontStyle

    #region SimpleFontStyleCaps

    public enum SimpleFontStyleCaps
    {
        None = 0,
        SmallCaps = 1,
        AllCaps = 2,
    }

    #endregion SimpleFontStyleCaps

    #region SimpleFontStyleScript

    public enum SimpleFontStyleScript
    {
        None = 0,
        Subscript = 1,
        Superscript = 2
    }

    #endregion SimpleFontStyleScript

    #region SimpleFontStyleFomratting

    [Flags]
    public enum SimpleFontStyleFormatting
    {
        Unspecified = 1,
        None = 2,
        Italic = 4,
        Bold = 8,
        Underlined = 16
    }

    #endregion SimpleFontStyleFormatting
}

namespace System.IO
{
    #region AccessRights

    public enum FileAccessRights
    {
        Unknown,
        FileAttributeReadOnly,
        None,
        Read,
        Write
    }

    #endregion
}