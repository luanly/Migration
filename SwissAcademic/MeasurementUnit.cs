using System;

namespace SwissAcademic
{
    public class MeasurementUnit
    {
        #region Felder

        private static MeasurementUnit _centimeters = new MeasurementUnit(MeasurementUnitType.Centimeters);
        private static MeasurementUnit _millimeters = new MeasurementUnit(MeasurementUnitType.Millimeters);
        private static MeasurementUnit _inches = new MeasurementUnit(MeasurementUnitType.Inches);
        private static MeasurementUnit _numeric = new MeasurementUnit(MeasurementUnitType.Numeric);
        private static MeasurementUnit _points = new MeasurementUnit(MeasurementUnitType.Points);
        private static MeasurementUnit _twips = new MeasurementUnit(MeasurementUnitType.Twips);

        private string _abbreviation;
        private MeasurementUnitType _measurementUnitType;
        private string _name;

        #endregion

        #region Konstruktoren

        private MeasurementUnit(MeasurementUnitType measurementUnitType)
        {
            _measurementUnitType = measurementUnitType;

            switch (measurementUnitType)
            {
                case MeasurementUnitType.Centimeters:
                    _name = "Centimeters";
                    _abbreviation = "cm";
                    break;

                case MeasurementUnitType.Inches:
                    _name = "Inches";
                    _abbreviation = "in";
                    break;

                case MeasurementUnitType.Millimeters:
                    _name = "Millimeters";
                    _abbreviation = "mm";
                    break;

                case MeasurementUnitType.Numeric:
                    _name = "Numeric";
                    _abbreviation = "";
                    break;

                case MeasurementUnitType.Points:
                    _name = "Points";
                    _abbreviation = "pt";
                    break;

                case MeasurementUnitType.Twips:
                    _name = "Twips";
                    _abbreviation = "tw";
                    break;
            }
        }

        #endregion

        #region Eigenschaften

        //TODO JHP Precision Scale
        //http://stackoverflow.com/questions/763942/calculate-system-decimal-precision-and-scale

        #region Abbreviation

        public string Abbreviation
        {
            get { return _abbreviation; }
        }

        #endregion

        #region Centimeters

        public static MeasurementUnit Centimeters
        {
            get { return _centimeters; }
        }

        #endregion

        #region Inches

        public static MeasurementUnit Inches
        {
            get { return _inches; }
        }

        #endregion

        #region MeasurementUnitType

        public MeasurementUnitType MeasurementUnitType
        {
            get { return _measurementUnitType; }
        }

        #endregion

        #region Name

        public string Name
        {
            get { return _name; }
        }

        #endregion

        #region Millimeters

        public static MeasurementUnit Millimeters
        {
            get { return _millimeters; }
        }

        #endregion

        #region Numeric

        public static MeasurementUnit Numeric
        {
            get { return _numeric; }
        }

        #endregion

        #region Points

        public static MeasurementUnit Points
        {
            get { return _points; }
        }

        #endregion

        #region Twips

        public static MeasurementUnit Twips
        {
            get { return _twips; }
        }

        #endregion

        #endregion

        #region Methoden

        #region ConvertValue

        public float ConvertValue(float value, MeasurementUnitType targetUnitType)
        {
            if (targetUnitType == MeasurementUnitType)
                return value;

            if (_measurementUnitType == MeasurementUnitType.Numeric || targetUnitType == MeasurementUnitType.Numeric)
                return value;

            switch (_measurementUnitType)
            {
                case MeasurementUnitType.Centimeters:
                    switch (targetUnitType)
                    {
                        case MeasurementUnitType.Millimeters:
                            return value * 10F;

                        case MeasurementUnitType.Inches:
                            return value * 0.393700787401575F;

                        case MeasurementUnitType.Points:
                            return value * 28.3464566929134F;

                        case MeasurementUnitType.Twips:
                            return value * 566.929133858268F;

                        default:
                            throw new ApplicationException("Undefined Conversion");
                    }

                case MeasurementUnitType.Inches:
                    switch (targetUnitType)
                    {
                        case MeasurementUnitType.Centimeters:
                            return value * 2.54F;

                        case MeasurementUnitType.Millimeters:
                            return value * 25.4F;

                        case MeasurementUnitType.Points:
                            return value * 72F;

                        case MeasurementUnitType.Twips:
                            return value * 1440F;

                        default:
                            throw new ApplicationException("Undefined Conversion");
                    }

                case MeasurementUnitType.Millimeters:
                    switch (targetUnitType)
                    {
                        case MeasurementUnitType.Centimeters:
                            return value * 0.1F;

                        case MeasurementUnitType.Inches:
                            return value * 0.0393700787401575F;

                        case MeasurementUnitType.Points:
                            return value * 2.83464566929134F;

                        case MeasurementUnitType.Twips:
                            return value * 56.6929133858268F;

                        default:
                            throw new ApplicationException("Undefined Conversion");
                    }

                case MeasurementUnitType.Points:
                    switch (targetUnitType)
                    {
                        case MeasurementUnitType.Centimeters:
                            return value * 0.0352777777777778F;

                        case MeasurementUnitType.Inches:
                            return value * 0.0138888888888889F;

                        case MeasurementUnitType.Millimeters:
                            return value * 0.352777777777778F;

                        case MeasurementUnitType.Twips:
                            return value * 20F;

                        default:
                            throw new ApplicationException("Undefined Conversion");
                    }

                case MeasurementUnitType.Twips:
                    switch (targetUnitType)
                    {
                        case MeasurementUnitType.Centimeters:
                            return value * 0.00176388888888889F;

                        case MeasurementUnitType.Inches:
                            return value * 0.000694444444444445F;

                        case MeasurementUnitType.Millimeters:
                            return value * 0.0176388888888889F;

                        case MeasurementUnitType.Points:
                            return value * 0.05F;

                        default:
                            throw new ApplicationException("Undefined Conversion");
                    }

                default:
                    throw new ApplicationException("Undefined Conversion");
            }
        }

        #endregion

        #region FindMeasurementUnitType

        public static MeasurementUnit FindMeasurementUnitType(MeasurementUnitType measurementUnitType)
        {
            switch (measurementUnitType)
            {
                case MeasurementUnitType.Centimeters:
                    return _centimeters;

                case MeasurementUnitType.Inches:
                    return _inches;

                case MeasurementUnitType.Millimeters:
                    return _millimeters;

                case MeasurementUnitType.Numeric:
                    return _numeric;

                case MeasurementUnitType.Points:
                    return _points;

                case MeasurementUnitType.Twips:
                    return _twips;

                default:
                    throw new ApplicationException("Undefined MeasurementUnitType");
            }
        }

        #endregion

        #endregion
    }
}
