using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorProfiles
{
    internal static class ColorSpaces
    {
        internal static readonly ColorSpace sRGB = new SRGBColorSpace();
        internal static readonly ColorSpace wideGamut = new WideGamutColorSpace();
        internal static readonly ColorSpace adobeRGB = new AdobeRGBColorSpace();
        internal static readonly ColorSpace cieRgb = new CIERGBColorSpace();
        internal static readonly ColorSpace palSecam = new PALSECAMColorSpace();
        internal static readonly ColorSpace custom = new CustomColorSpace("Custom color space 1");
        internal static readonly ColorSpace custom2 = new CustomColorSpace("Custom color space 2");

        internal static readonly ObservableCollection<ColorSpace> ColorSpaceList = new ObservableCollection<ColorSpace>
            { sRGB, wideGamut, adobeRGB, cieRgb, palSecam, custom, custom2 };
    }
}
