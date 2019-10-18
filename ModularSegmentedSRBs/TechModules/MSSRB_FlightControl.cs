
using KSP_Log;

namespace ModularSegmentedSRBs.TechModules
{

    public class MSSRB_FlightControl : ModuleCommand
    {
        Log Log = new Log("ModularSegmentedSRBs.MSSRB_FlightControl");
        
        bool techPartResearched = false;

        public override void OnStart(StartState state)
        {
            Start();
            base.OnStart(state);
        }

        public override void Start()
        {
            techPartResearched = ModSegSRBs.PartAvailable(ModSegSRBs.MFCTechName);
            if (!techPartResearched)
            {
                if (HighLogic.LoadedScene != GameScenes.LOADING)
                {
                    part.RemoveModule(this);
                }
            }
            else
            {
                Log.Info(ModSegSRBs.MFCTechName + " researched");
            }

            base.Start();
        }

        public override void OnLoad(ConfigNode node)
        {
            Start();
            if ((techPartResearched || HighLogic.LoadedScene == GameScenes.LOADING) && node != null)
                base.OnLoad(node);
        }

        public override string GetInfo()
        {
            string st = base.GetInfo();
            return st;
        }

        public override string GetModuleDisplayName()
        {

            string st = base.GetModuleDisplayName();
            return st;
        }

        public override bool IsStageable() { return techPartResearched; }

        protected override void OnNetworkInitialised() { }
    }
}
