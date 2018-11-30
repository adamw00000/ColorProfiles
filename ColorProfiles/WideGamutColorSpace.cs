using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorProfiles
{
    class WideGamutColorSpace: ColorSpace
    {
        public WideGamutColorSpace(): base("Adobe Wide Gamut", false, 0.3457, 0.3585, 0.7347, 0.2653, 0.1152, 0.8264, 0.1566, 0.177, 1.2) { }
    }
}
