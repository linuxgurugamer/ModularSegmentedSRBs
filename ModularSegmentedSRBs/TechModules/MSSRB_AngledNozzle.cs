using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP_Log;

namespace ModularSegmentedSRBs.TechModules
{
    public class MSSRB_AngledNozzle: ModuleAnimateGeneric
    {
        const string TechNameTemplate = "MSSRB.<TAG>AngledNozzle";
        string TechName = "";
        Log Log = new Log("ModularSegmentedSRBs.MSSRB_ModuleParachute");

        static AvailablePart techPart;
        static bool techPartResearched = false;

        [KSPField]
        public string baseEngineName;

        public override void OnStart(StartState state)
        {
            Start();
            base.OnStart(state);
        }

        void Start()
        {
            TechName =  TechNameTemplate.Replace("<TAG>", animationName.Replace("Offset",""));
            actionGUIName = "Enable/disable angled nozzle";
            endEventGUIName = "Disable angled nozzle";
            startEventGUIName = "Enable angled nozzle";

            if (PartLoader.DoesPartExist(TechName))
            {
                techPart = PartLoader.getPartInfoByName(TechName);
                techPartResearched = PartResearched(techPart);
                if (!techPartResearched)
                {
                    if (HighLogic.LoadedScene != GameScenes.LOADING)
                    {
                        Log.Info(TechName + ", not researched yet");
                        part.RemoveModule(this);
                    }
                }
                else
                {
                    Log.Info("researched");
                }
            }
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
