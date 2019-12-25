using KSP_Log;

namespace ModularSegmentedSRBs
{
    class MSSRB_ModulePrecisionPropulsion : ModuleEnginesFX
    {
        KSP_Log.Log Log = new Log("ModularSegmentedSRBs.MSSRB_ModulePrecisionPropulsion");
        
        bool techPartResearched = false;

        public override void OnStart(StartState state)
        {
            Start();
            base.OnStart(state);
        }

        void Start()
        {
            Log.Info("Start");
            if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
            {
                techPartResearched = ModSegSRBs.PartAvailable(ModSegSRBs.PPTechName);
                Log.Info("Start, techPartResearched(" + ModSegSRBs.PPTechName + "): " + techPartResearched);
                if (!techPartResearched)
                {
                    // remove the fuel since it isn't being used
                    if (this.part != null)
                    {
                        PartResourceList prl = this.part.Resources;
                        if (prl.Contains(ModSegSRBs.SeparatronFuel))
                        {
                            part.RemoveResource(ModSegSRBs.SeparatronFuel);
                        }
                    }
                    if (HighLogic.LoadedScene != GameScenes.LOADING && HighLogic.LoadedScene != GameScenes.EDITOR)
                    {
                        Log.Info(ModSegSRBs.PPTechName + ", not researched yet");
                        part.RemoveModule(this);
                    }

                }
                else
                {
                    Log.Info(ModSegSRBs.PPTechName + " researched");
                }
            }
        }


        public override void OnLoad(ConfigNode node)
        {
            Start();
            if (techPartResearched || HighLogic.LoadedScene == GameScenes.LOADING || HighLogic.LoadedScene == GameScenes.EDITOR)
                base.OnLoad(node);
        }

        public override void OnAwake()
        {
            Start();
            if (techPartResearched || HighLogic.LoadedScene == GameScenes.LOADING || HighLogic.LoadedScene == GameScenes.EDITOR)
                base.OnAwake();
        }

        public override string GetInfo()
        {
            string st = "";
            if (techPartResearched)
                st = base.GetInfo();
            return st;
        }

        public override string GetModuleDisplayName()
        {
            string st = "";
            if (techPartResearched)
                st = base.GetModuleDisplayName();
            return st;
        }

        public override bool IsStageable()
        {
            if (techPartResearched)
                return base.IsStageable();
            return false;
        }
    }
}
