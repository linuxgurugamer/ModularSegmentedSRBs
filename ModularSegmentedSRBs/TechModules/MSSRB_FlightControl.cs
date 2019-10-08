﻿
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
                if (techPart == null)
                {
                    Log.Error("Start, TechName NOT found: " + TechName);
                    base.OnDestroy();
                    Destroy(this);
                }
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
            if ((techPartResearched || HighLogic.LoadedScene == GameScenes.LOADING) && node != null)
                base.OnLoad(node);
        }

#if false
        public override void OnAwake()
        {
            Start();
            if (techPartResearched || HighLogic.LoadedScene == GameScenes.LOADING)
                base.OnAwake();
        }
#endif

        public bool PartResearched(AvailablePart p)
        {
            if (p == null)
            {
                Log.Error("PartResearched, AvailablePart is null");
            }
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

        protected override void OnNetworkInitialised() { }
    }
}
