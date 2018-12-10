using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorProfiles
{
    class PALSECAMColorSpace: ColorSpace
    {
        public PALSECAMColorSpace() : base("PAL/SECAM", false, 0.3127, 0.329, 0.64, 0.33, 0.29, 0.6, 0.15, 0.06, 1.95) { }
    }
}
