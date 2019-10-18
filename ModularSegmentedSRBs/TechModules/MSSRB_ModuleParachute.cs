using UnityEngine;
using KSP_Log;

namespace ModularSegmentedSRBs
{

    public class MSSRB_ModuleParachute : ModuleParachute
    {
        Log Log = new Log("ModularSegmentedSRBs.MSSRB_ModuleParachute");
        
        bool techPartResearched = false;

        [UI_FloatRange(stepIncrement = 10f, maxValue = 300f, minValue = 0f)]
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Deployment Delay (secs)")]
        public float deploymentDelay = 20f;


        float deploymentDelayElapsedTime = 0;

        public override void OnStart(StartState state)
        {
            Start();
            base.OnStart(state);
        }

        void Start()
        {
            techPartResearched = ModSegSRBs.PartAvailable(ModSegSRBs.ParachuteTechName);
            if (!techPartResearched)
            {
                if (HighLogic.LoadedScene != GameScenes.LOADING)
                {
                    Log.Info(ModSegSRBs.ParachuteTechName + ", not researched yet");
                    part.RemoveModule(this);
                }
            }
            else
            {
                Log.Info(ModSegSRBs.ParachuteTechName + "researched");
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
