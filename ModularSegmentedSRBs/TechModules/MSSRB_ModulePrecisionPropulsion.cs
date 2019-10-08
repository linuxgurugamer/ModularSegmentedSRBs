using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP_Log;

namespace ModularSegmentedSRBs
{
    class MSSRB_ModulePrecisionPropulsion : ModuleEnginesFX
    {
        const string TechName = "MSSRB.PrecisionPropulsion";
        Log Log = new Log("ModularSegmentedSRBs.MSSRB_ModulePrecisionPropulsion");

        static AvailablePart techPart;
        static bool techPartResearched = false;

        public override void OnStart(StartState state)
        {
            Start();
            base.OnStart(state);
        }

        void Start()
        {   
            if (PartLoader.DoesPartExist(TechName))
            {
                Log.Info("Start, Part exists: " + TechName);
                techPart = PartLoader.getPartInfoByName(TechName);
                techPartResearched = PartResearched(techPart);
                if (!techPartResearched)
                {
                    // remove the fuel since it isn't being used
                    if (this.part != null)
                    {
                        PartResourceList prl = this.part.Resources;
                        if (prl.Contains(ModSegSRBs.SeparatronFuel))
                            //prl.Remove(ModSegSRBs.SeparatronFuel);
                            prl[ModSegSRBs.SeparatronFuel].amount = prl[ModSegSRBs.SeparatronFuel].maxAmount = 0;
                    }
                    if (HighLogic.LoadedScene != GameScenes.LOADING && HighLogic.LoadedScene != GameScenes.EDITOR)
                    {
                        Log.Info(TechName + ", not researched yet");
                        
                        Destroy(this);
                    }
                }
            } else
            {
                Log.Error("TechName NOT found: " + TechName);
            }
        }

        public bool PartResearched(AvailablePart p)
        {
            return ResearchAndDevelopment.PartTechAvailable(p) && ResearchAndDevelopment.PartModelPurchased(p);
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
