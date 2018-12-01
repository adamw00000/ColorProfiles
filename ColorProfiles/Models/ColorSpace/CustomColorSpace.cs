using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorProfiles
{
    class CustomColorSpace: ColorSpace
    {
        public CustomColorSpace(): base("Custom color space", true, 0.3127, 0.3290, 0.64, 0.33, 0.3, 0.6, 0.15, 0.06, 2.2) { }
        public CustomColorSpace(string name) : base(name, true, 0.3127, 0.3290, 0.64, 0.33, 0.3, 0.6, 0.15, 0.06, 2.2) { }
    }
}
