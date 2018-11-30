using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorProfiles
{
    internal static class ColorSpaces
    {
        internal static readonly ColorSpace sRGB = new SRGBColorSpace();
        internal static readonly ColorSpace wideGamut = new WideGamutColorSpace();
        internal static readonly ColorSpace custom = new CustomColorSpace();
    }
}
