using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorProfiles
{
    class CIERGBColorSpace: ColorSpace
    {
        public CIERGBColorSpace() : base("CIE RGB", false, 0.3333, 0.3333, 0.735, 0.265, 0.274, 0.717, 0.167, 0.009, 2.2) { }
    }
}
