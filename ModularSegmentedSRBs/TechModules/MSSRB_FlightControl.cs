
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP_Log;

namespace ModularSegmentedSRBs.TechModules
{

    public class MSSRB_FlightControl : ModuleCommand
    {
        const string TechName = "MSSRB.ModuleFlightControl";
        Log Log = new Log("ModularSegmentedSRBs.MSSRB_FlightControl");

        static AvailablePart techPart;
        static bool techPartResearched = false;

        public override void OnStart(StartState state)
        {
            Start();
            base.OnStart(state);
        }

        public override void Start()
        {

            if (PartLoader.DoesPartExist(TechName))
            {
                techPart = PartLoader.getPartInfoByName(TechName);
                techPartResearched = PartResearched(techPart);
                if (!techPartResearched)
                {
                    if (HighLogic.LoadedScene != GameScenes.LOADING)
                    {
                        base.OnDestroy();
                        Destroy(this);
                    }
                }
                else
                {
                    Log.Info("researched");
                }
            }
            base.Start();
        }
        public override void OnLoad(ConfigNode node)
        {
            Start();
            if (techPartResearched || HighLogic.LoadedScene == GameScenes.LOADING)
                base.OnLoad(node);
        }
        public override void OnAwake()
        {
            Start();
            if (techPartResearched || HighLogic.LoadedScene == GameScenes.LOADING)
                base.OnAwake();
        }
        public bool PartResearched(AvailablePart p)
        {
            return ResearchAndDevelopment.PartTechAvailable(p) && ResearchAndDevelopment.PartModelPurchased(p);
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

    }
}
