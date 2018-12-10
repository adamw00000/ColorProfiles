using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorProfiles
{
    class AdobeRGBColorSpace: ColorSpace
    {
        public AdobeRGBColorSpace() : base("Adobe RGB", false, 0.3127, 0.329, 0.64, 0.33, 0.21, 0.71, 0.15, 0.06, 2.2) { }
    }
}
