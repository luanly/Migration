using System.Drawing;

namespace SwissAcademic
{
    public class Hsl
    {
        #region Konstruktoren

        public Hsl()
        {
        }

        #endregion

        #region Eigenschaften

        #region H

        double _h;

        public double H
        {
            get { return _h; }

            set
            {
                _h = value;
                _h = _h > 1 ? 1 : _h < 0 ? 0 : _h;
            }

        }

        #endregion

        #region S

        double _s;

        public double S
        {
            get { return _s; }

            set
            {
                _s = value;
                _s = _s > 1 ? 1 : _s < 0 ? 0 : _s;
            }
        }

        #endregion

        #region L

        double _l;

        public double L
        {
            get { return _l; }

            set
            {
                _l = value;
                _l = _l > 1 ? 1 : _l < 0 ? 0 : _l;
            }
        }

        #endregion

        #endregion
    }

    public static class RgbHsl
    {
        #region Methoden

        #region ToRgb

        public static Color ToRgb(this Hsl hsl)
        {
            double r = 0, g = 0, b = 0;
            double temp1, temp2;

            if (hsl.L == 0)
            {
                r = g = b = 0;
            }

            else
            {
                if (hsl.S == 0)
                {
                    r = g = b = hsl.L;
                }

                else
                {
                    temp2 = ((hsl.L <= 0.5) ? hsl.L * (1.0 + hsl.S) : hsl.L + hsl.S - (hsl.L * hsl.S));
                    temp1 = 2.0 * hsl.L - temp2;

                    double[] t3 = new double[] { hsl.H + 1.0 / 3.0, hsl.H, hsl.H - 1.0 / 3.0 };
                    double[] clr = new double[] { 0, 0, 0 };

                    for (int i = 0; i < 3; i++)
                    {
                        if (t3[i] < 0) t3[i] += 1.0;
                        if (t3[i] > 1) t3[i] -= 1.0;

                        if (6.0 * t3[i] < 1.0) clr[i] = temp1 + (temp2 - temp1) * t3[i] * 6.0;
                        else if (2.0 * t3[i] < 1.0) clr[i] = temp2;
                        else if (3.0 * t3[i] < 2.0) clr[i] = (temp1 + (temp2 - temp1) * ((2.0 / 3.0) - t3[i]) * 6.0);
                        else clr[i] = temp1;
                    }

                    r = clr[0];
                    g = clr[1];
                    b = clr[2];
                }
            }

            return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }

        #endregion

        #region ModifyBrightness

        public static Color ModifyBrightness(this Color color, double brightness)
        {
            var hsl = color.ToHsl();
            hsl.L *= brightness;
            return hsl.ToRgb();
        }

        #endregion

        #region ModifyHue

        public static Color ModifyHue(this Color color, double hue)
        {
            var hsl = color.ToHsl();
            hsl.H *= hue;
            return hsl.ToRgb();
        }

        #endregion

        #region ModifySaturation

        public static Color ModifySaturation(this Color color, double saturation)
        {
            var hsl = color.ToHsl();
            hsl.S *= saturation;
            return hsl.ToRgb();
        }

        #endregion

        #region RgbToHsl

        public static Hsl ToHsl(this Color color)
        {
            Hsl hsl = new Hsl();

            hsl.H = color.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360 
            hsl.L = color.GetBrightness();
            hsl.S = color.GetSaturation();

            return hsl;
        }

        #endregion

        #region SetBrightness

        public static Color SetBrightness(this Color color, double brightness)
        {
            var hsl = color.ToHsl();
            hsl.L = brightness;
            return hsl.ToRgb();
        }

        #endregion

        #region SetHue

        public static Color SetHue(this Color color, double hue)
        {
            var hsl = color.ToHsl();
            hsl.H = hue;
            return hsl.ToRgb();
        }

        #endregion

        #region SetSaturation

        public static Color SetSaturation(this Color color, double saturation)
        {
            var hsl = color.ToHsl();
            hsl.S = saturation;
            return hsl.ToRgb();
        }

        #endregion

        #endregion
    }
}


