using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP_Log;

namespace ModularSegmentedSRBs
{
    class MS_SRB_SegmentEnds : ModuleEnginesFX
    {
        [KSPField]
        public string attachNode = "none";

#if false
        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Tmp Activate")]
        public void DoActivate()
        {
            enabled = true;

            part.Resources[ModSegSRBs.BurnablePropellant].maxAmount =
                                part.Resources[ModSegSRBs.BurnablePropellant].amount = part.Resources[ModSegSRBs.Propellant].amount + part.Resources[ModSegSRBs.BurnablePropellant].amount;
            part.Resources[ModSegSRBs.Propellant].amount = 0;
  
            base.Activate();
        }
#endif

        public override bool IsStageable() { return false; }

        internal MS_SRB_Engine baseEngine; // This is set in the MS_SRB_Engine module

        AttachNode attNode;
        MS_SRB_Fuel_Segment thisSegment;
        MS_SRB_Fuel_Segment attachedSegment;

        Log Log = new Log("ModularSegmentedSRBs.MS_SRB_SegmentEnds");
        void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                attNode = this.part.FindAttachNode(attachNode);
                if (base.Events.Contains("Activate"))
                {
                    base.Events["Activate"].active = false;
                    base.Events["Activate"].guiActive = false;
                }
                if (base.Events.Contains("Shutdown"))
                {
                    base.Events["Shutdown"].active = false;
                    base.Events["Shutdown"].guiActive = false;
                }
                try
                {
                    base.Fields["independentThrottle"].guiActive = false;
                    base.Fields["independentThrottle"].guiActiveEditor = false;
                }
                catch { Log.Info("Field not found: " + independentThrottle); }
                try
                {

                    base.Fields["independentThrottlePercentage"].guiActive = false;
                    base.Fields["independentThrottlePercentage"].guiActiveEditor = false;
                }
                catch { Log.Info("Field not found: " + independentThrottlePercentage); }
                try
                {
                    base.Fields["thrustPercentage"].guiActive = false;
                    base.Fields["thrustPercentage"].guiActiveEditor = false;
                }
                catch { Log.Info("Field not found: " + thrustPercentage); }

                PartModuleList pml = part.Modules;
                if (pml.Contains<MS_SRB_Fuel_Segment>())
                {
                    thisSegment = pml.GetModule<MS_SRB_Fuel_Segment>();
                }
                if (attNode != null && attNode.attachedPart != null)
                {
                    pml = attNode.attachedPart.Modules;
                    if (pml != null && pml.Contains<MS_SRB_Fuel_Segment>())
                    {
                        attachedSegment = pml.GetModule<MS_SRB_Fuel_Segment>();
                    }
                }
            }
        }

#if false
        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Debug Info")]
        void DebugInfo()
        {
            Log.Info("part id: " + part.persistentId + ", attachNode: " + attachNode);
            if (attNode != null && attNode.attachedPart != null)
                Log.Info("attached part: " + attNode.attachedPart.persistentId);
            else
                Log.Info("No attached part");
            Log.Info("maxThrust: " + maxThrust);
            Log.Info("atmosphereCurve: " + atmosphereCurve);
            foreach (var r in part.Resources)
            {
                Log.Info("Resource: " + r.resourceName + ", maxAmount: " + r.maxAmount + ", amount: " + r.amount);
            }

        }
#endif



        bool activated = false;
        void ActivateEngine(string s)
        {
            if (HighLogic.LoadedSceneIsFlight && !activated)
            {
                baseEngine.UpdateSegments();
                baseEngine.SetEngineHealth(0.1f);
                this.maxThrust = baseEngine.maxThrust;
                this.atmosphereCurve = baseEngine.atmosphereCurve;

                baseEngine.ChangeUsage(0.1f, ref this.maxThrust, ref this.atmosphereCurve);

                enabled = true;
                activated = true;
                part.Resources[ModSegSRBs.BurnablePropellant].maxAmount =
                               part.Resources[ModSegSRBs.BurnablePropellant].amount = part.Resources[ModSegSRBs.Propellant].amount + part.Resources[ModSegSRBs.BurnablePropellant].amount;
                part.Resources[ModSegSRBs.Propellant].amount = 0;
                base.Activate();
            }
        }

        public override void Activate()
        {
            // Makes sure that this isn't activated when the baseEngine is
            // Just leaves this as a do-nothing, otherwise the StageManager will activate this stage
        }

        new void FixedUpdate()
        {
            base.FixedUpdate();
            if (HighLogic.LoadedSceneIsFlight && baseEngine != null && baseEngine.EngineIgnited)
            {
                if (attNode.attachedPart == null && !EngineIgnited)
                {
                    ActivateEngine("attachedPart is null");
                }
                else
                {
                    if (attachedSegment != null && thisSegment.segmentWidth > attachedSegment.segmentWidth)
                        ActivateEngine("This part wider than attached part");
                }
            }
        }
    }
}
