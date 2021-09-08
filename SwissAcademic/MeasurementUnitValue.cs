using System;

namespace SwissAcademic
{
    public struct MeasurementUnitValue
    {
        #region Felder

        private int _value;     //immer in Twips

        #endregion

        #region Konstruktoren

        public MeasurementUnitValue(float value, MeasurementUnitType measurementUnitType)
        {
            switch (measurementUnitType)
            {
                case MeasurementUnitType.Centimeters:
                    _value = Convert.ToInt32(MeasurementUnit.Centimeters.ConvertValue(value, MeasurementUnitType.Twips));
                    break;

                case MeasurementUnitType.Inches:
                    _value = Convert.ToInt32(MeasurementUnit.Inches.ConvertValue(value, MeasurementUnitType.Twips));
                    break;

                case MeasurementUnitType.Millimeters:
                    _value = Convert.ToInt32(MeasurementUnit.Millimeters.ConvertValue(value, MeasurementUnitType.Twips));
                    break;

                case MeasurementUnitType.Points:
                    _value = Convert.ToInt32(MeasurementUnit.Points.ConvertValue(value, MeasurementUnitType.Twips));
                    break;

                case MeasurementUnitType.Numeric:
                case MeasurementUnitType.Twips:
                default:
                    _value = Convert.ToInt32(value);
                    break;
            }
        }

        #endregion

        #region Eigenschaften

        #region CentimeterValue

        public float CentimeterValue
        {
            get { return MeasurementUnit.Twips.ConvertValue(_value, MeasurementUnitType.Centimeters); }
            set { _value = Convert.ToInt32(MeasurementUnit.Centimeters.ConvertValue(value, MeasurementUnitType.Twips)); }
        }

        #endregion

        #region InchValue

        public float InchValue
        {
            get { return MeasurementUnit.Twips.ConvertValue(_value, MeasurementUnitType.Inches); }
            set { _value = Convert.ToInt32(MeasurementUnit.Inches.ConvertValue(value, MeasurementUnitType.Twips)); }
        }

        #endregion

        #region MillimeterValue

        public float MillimeterValue
        {
            get { return MeasurementUnit.Twips.ConvertValue(_value, MeasurementUnitType.Millimeters); }
            set { _value = Convert.ToInt32(MeasurementUnit.Millimeters.ConvertValue(value, MeasurementUnitType.Twips)); }
        }

        #endregion

        #region NumericValue

        public int NumericValue
        {
            get { return _value; }
            set { _value = value; }
        }

        #endregion

        #region PointValue

        public float PointValue
        {
            get { return MeasurementUnit.Twips.ConvertValue(_value, MeasurementUnitType.Points); }
            set { _value = Convert.ToInt32(MeasurementUnit.Points.ConvertValue(value, MeasurementUnitType.Twips)); }
        }

        #endregion

        #region TwipsValue

        public int TwipsValue
        {
            get { return _value; }
            set { _value = value; }
        }

        #endregion

        #endregion

        #region Methoden

        public float GetValue(MeasurementUnitType measurementUnitType)
        {
            switch (measurementUnitType)
            {
                case MeasurementUnitType.Centimeters:
                    return CentimeterValue;

                case MeasurementUnitType.Inches:
                    return InchValue;

                case MeasurementUnitType.Millimeters:
                    return MillimeterValue;

                case MeasurementUnitType.Points:
                    return PointValue;

                case MeasurementUnitType.Twips:
                    return TwipsValue;

                case MeasurementUnitType.Numeric:
                default:
                    return NumericValue;
            }
        }

        #endregion
    }
}
