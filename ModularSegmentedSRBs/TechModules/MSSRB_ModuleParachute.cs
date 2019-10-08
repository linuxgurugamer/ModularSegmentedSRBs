using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP_Log;

namespace ModularSegmentedSRBs
{

    public class MSSRB_ModuleParachute : ModuleParachute
    {
        const string TechName = "MSSRB.Parachute";
        Log Log = new Log("ModularSegmentedSRBs.MSSRB_ModuleParachute");

        static AvailablePart techPart;
        static bool techPartResearched = false;

        [UI_FloatRange(stepIncrement = 10f, maxValue = 300f, minValue = 0f)]
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Deployment Delay (secs)")]
        public float deploymentDelay = 20f;


        public override void OnStart(StartState state)
        {
            Start();
            base.OnStart(state);
        }

        void Start()
        {
            if (PartLoader.DoesPartExist(TechName))
            {
                techPart = PartLoader.getPartInfoByName(TechName);
                techPartResearched = PartResearched(techPart);
                if (!techPartResearched)
                {
                    if (HighLogic.LoadedScene != GameScenes.LOADING)
                    {
                        Log.Info(TechName + ", not researched yet");

                        base.OnDestroy();
                        Destroy(this);
                    }
                }
                else
                {
                    Log.Info("researched");
                }
            }
            else
                Log.Error("Start, TechName NOT found: " + TechName);
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

        float deploymentDelayElapsedTime = 0;
        public new void FixedUpdate()
        {
            if (deploymentState > 0)
            {
                deploymentDelayElapsedTime = deploymentDelayElapsedTime + Time.fixedDeltaTime;
                if (deploymentDelayElapsedTime < deploymentDelay)
                    return;

            }
            base.FixedUpdate();
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
