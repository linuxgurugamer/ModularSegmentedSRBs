using KSP_Log;

namespace ModularSegmentedSRBs
{
    class MSSRB_Endcap : PartModule
    {
        public new MSSRB_Part part { get { return base.part as MSSRB_Part; } }

        Log Log = new Log("ModularSegmentedSRBs.MSSRB_Fuel_Segment");

        public void Start()
        {
            Log.Info("MSSRB_Endcap.Start");
        }
    }
}