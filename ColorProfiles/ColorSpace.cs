using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using StarMathLib;

namespace ColorProfiles
{
    class ColorSpace
    {
        string Name { get; }
#pragma warning disable IDE1006 // Naming Styles
        public double xw { get; set; }
        public double yw { get; set; }
        public double xr { get; set; }
        public double yr { get; set; }
        public double xg { get; set; }
        public double yg { get; set; }
        public double xb { get; set; }
        public double yb { get; set; }
        public double gamma { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public bool Editable { get; }
        readonly double[,] RGBToXYZMatrix;
        readonly double[,] XYZToRGBMatrix;

        protected ColorSpace(string name, bool editable, 
            double xw, double yw, 
            double xr, double yr, 
            double xg, double yg, 
            double xb, double yb, 
            double gamma)
        {
            Name = name;
            Editable = editable;
            this.xw = xw;
            this.yw = yw;
            this.xr = xr;
            this.yr = yr;
            this.xg = xg;
            this.yg = yg;
            this.xb = xb;
            this.yb = yb;

            this.gamma = gamma;

            double zr, zg, zb, zw;
            double Yw = 1;
            double Xw, Zw;

            zw = 1 - xw - yw;
            zr = 1 - xr - yr;
            zg = 1 - xg - yg;
            zb = 1 - xb - yb;

            double S = Yw / yw;
            Xw = xw * S;
            Zw = (1 - xw - yw) * S;

            double[,] mXYZrgb = { { xr, xg, xb }, { yr, yg, yb }, { zr, zg, zb } };
            var mXYZrgbInverted = mXYZrgb.inverse();
            double[] vXYZw = { Xw, Yw, Zw };

            var vSrgb = StarMath.multiply(mXYZrgbInverted, vXYZw);

            double[,] mSXSYSZrgb = { 
                { vSrgb[0] * xr, vSrgb[1] * xg, vSrgb[2] * xb }, 
                { vSrgb[0] * yr, vSrgb[1] * yg, vSrgb[2] * yb }, 
                { vSrgb[0] * zr, vSrgb[1] * zg, vSrgb[2] * zb }
            };

            RGBToXYZMatrix = mSXSYSZrgb;
            XYZToRGBMatrix = mSXSYSZrgb.inverse();
        }

        public Vector3D RGBToXYZ(Color c)
        {
            double[] vRGB = { Math.Pow(c.R / 255d, gamma), Math.Pow(c.G / 255d, gamma), Math.Pow(c.B / 255d, gamma) };

            var result = StarMath.multiply(RGBToXYZMatrix, vRGB);
            return new Vector3D(result[0], result[1], result[2]);
        }

        public Color XYZToRGB(Vector3D v)
        {
            double[] vXYZ = { v.X, v.Y, v.Z };

            var result = StarMath.multiply(XYZToRGBMatrix, vXYZ);
            double R = Math.Pow(result[0], 1 / gamma) * 255;
            double G = Math.Pow(result[1], 1 / gamma) * 255;
            double B = Math.Pow(result[2], 1 / gamma) * 255;

            return Color.FromArgb(255,
                (byte)Math.Min(R, 255),
                (byte)Math.Min(G, 255),
                (byte)Math.Min(B, 255));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
