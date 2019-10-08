using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModularSegmentedSRBs
{

    class MS_SRB_Endcap : PartModule
    {
        [KSPField(isPersistant = true)]
        public float segmentHeight = 0.5f;

        [KSPField(isPersistant = true)]
        public float segmentWidth = 1.25f;
    }
}