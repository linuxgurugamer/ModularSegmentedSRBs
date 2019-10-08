using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modular_Segmented_SRBs
{
    class MS_SRB_Nosecone : PartModule
    {
        [KSPField(isPersistant = true)]
        public float segmentHeight = 1.25f;

        [KSPField(isPersistant = true)]
        public float segmentWidth = 1.25f;

        [KSPAction("Abort!", actionGroup = KSPActionGroup.Abort)]
        public void DoJettison(KSPActionParam param)
        {
            part.decouple();
        }
    }
}
