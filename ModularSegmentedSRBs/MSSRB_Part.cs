using UnityEngine;
using KSP_Log;

namespace ModularSegmentedSRBs
{

    public class MSSRB_Part : Part
    {
        [KSPField(isPersistant = true)]
        public float segmentHeight = 0f;

        [KSPField(isPersistant = true)]
        public float segmentWidth = 0f;


        internal MSSRB_Part partInstantiatedFlag = null;

        Log Log = new Log("ModularSegmentedSRBs.MSSRB_Part");

        public new void Awake()
        {
            partInstantiatedFlag = this;
         
            base.Awake();
        }

        public void RestoreVariant()
        {
            Log.Info("Restoring variant: GetSelectedVariant(): " + GetSelectedVariant());
        }

        public string GetSelectedVariant()
        {
            ModulePartVariants mpv = Modules.GetModule<ModulePartVariants>();
            ModSegSRBs.GetExtraInfo(mpv.SelectedVariant, ref segmentHeight, ref segmentWidth);
            Log.Info("Variant height, width: " + segmentHeight + ", " + segmentWidth);

            return mpv.SelectedVariant.Name;
        }
#if false
        public void Start()
        {
            Debug.Log("MSSRB_Part");
            GetDimensionVariant();
        }
#endif

    }
}