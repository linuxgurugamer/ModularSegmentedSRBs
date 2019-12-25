using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP_Log;

namespace ModularSegmentedSRBs
{
    class MSSRB_SegmentEnds : ModuleEnginesFX
    {
        public new MSSRB_Part part { get { return base.part as MSSRB_Part; } }

        internal MSSRB_Engine baseEngine = null; // This is set in the MSSRB_Engine module

        [KSPField]
        public string attachNode = null;

        public override bool IsStageable() { return false; }



        AttachNode attNode;
        MSSRB_Fuel_Segment thisSegment;
        MSSRB_Fuel_Segment attachedSegment = null;
        MSSRB_Endcap attachedEndCap = null;
        MSSRB_Engine attachedMotor = null;

        bool activated = false;

        double totalFuelMass = 0;
        float totalMaxThrust = 0;

        Log Log = new Log("ModularSegmentedSRBs.MSSRB_SegmentEnds");

        void Start()
        {
            nonThrustMotor = true;
            attNode = this.part.FindAttachNode(attachNode);
            activated = false;
#if true

            base.Events["Activate"].active = false;
            base.Events["Activate"].guiActive = false;

            base.Events["Shutdown"].active = false;
            base.Events["Shutdown"].guiActive = false;

            base.Fields["independentThrottle"].guiActive = false;
            base.Fields["independentThrottle"].guiActiveEditor = false;

            base.Fields["independentThrottlePercentage"].guiActive = false;
            base.Fields["independentThrottlePercentage"].guiActiveEditor = false;

            base.Fields["thrustPercentage"].guiActive = false;
            base.Fields["thrustPercentage"].guiActiveEditor = false;

            Actions["ToggleThrottle"].active = false;
            Actions["ToggleThrottle"].activeEditor = false;



            foreach (var e in Events)
                Log.Info("Event: " + e.name + ", guiName = " + e.GUIName);
            foreach (var a in Actions)
                Log.Info("Action: " + a.name + ", guiName: " + a.guiName);
#endif
            if (HighLogic.LoadedSceneIsFlight)
            {

                PartModuleList pml = part.Modules;
                if (pml.Contains<MSSRB_Fuel_Segment>())
                    thisSegment = pml.GetModule<MSSRB_Fuel_Segment>();

                if (attNode != null && attNode.attachedPart != null)
                {
                    pml = attNode.attachedPart.Modules;
                    if (pml != null)
                    {
                        if (pml.Contains<MSSRB_Fuel_Segment>())
                            attachedSegment = pml.GetModule<MSSRB_Fuel_Segment>();

                        if (pml.Contains<MSSRB_Endcap>())
                            attachedEndCap = pml.GetModule<MSSRB_Endcap>();

                        if (pml.Contains<MSSRB_Engine>())
                            attachedMotor = pml.GetModule<MSSRB_Engine>();
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

        bool CheckPartResources(MSSRB_Part part, bool max = true)
        {
            // Check the nextPart's resources and Modules
            // Check for the correct propellant and that it has the MSSRB_Fuel_Segment module
            // If so, add it to the segments list and accumulate some values for the motor
            // then update the propellant value amount in the fuel segment
            PartResourceList prl = part.Resources;
            PartModuleList pml = part.Modules;
            if (prl.Contains(ModSegSRBs.Propellant) && pml.Contains<MSSRB_Fuel_Segment>())
            {
                Log.Info("CheckPartResources, " + ModSegSRBs.Propellant + " found, " + " MSSRB_Fuel_Segment found");
                MSSRB_Fuel_Segment moduleFuelSegment = pml["MSSRB_Fuel_Segment"] as MSSRB_Fuel_Segment;

                if (max)
                    totalFuelMass += moduleFuelSegment.MaxSolidFuel();  // prl[ModSegSRBs.Propellant].maxAmount;                
                else
                    totalFuelMass += prl[ModSegSRBs.Propellant].amount;
                totalMaxThrust += moduleFuelSegment.GetMaxThrust();


                prl[ModSegSRBs.Propellant].amount = moduleFuelSegment.MaxSolidFuel();
                prl[ModSegSRBs.BurnablePropellant].amount = 0;

                prl[ModSegSRBs.Propellant].maxAmount =
                    prl[ModSegSRBs.BurnablePropellant].maxAmount = moduleFuelSegment.MaxSolidFuel();
                Log.Info("totalMaxThrust: " + totalMaxThrust + ", totalFuelMass: " + totalFuelMass);


                return true;

            }
            else
            {
                Log.Info("CheckPartResources, no Propellant found");
                return false;
            }
        }

        double GetAllAvailableFuel(bool max = true)
        {
            string node = "top", attachedPartNode = "bottom";
            AttachNode attachNode = this.part.FindAttachNode(node);
            if (attachNode == null || attachNode.attachedPart == null)
            {
                node = "bottom";
                attachedPartNode = "top";
                attachNode = this.part.FindAttachNode(node);
            }

            Part curPart = this.part;
            Part nextPart;
            totalFuelMass = 0;
            totalMaxThrust = 0;
            CheckPartResources(this.part as MSSRB_Part, max);
            if (attachNode == null)
                return totalFuelMass;
            while (attachNode.attachedPart != null)
            {
                nextPart = attachNode.attachedPart;

                // Make sure that the part attached to this part's topNode is that part's bottom node
                AttachNode nextPartBottomNode = nextPart.FindAttachNode(attachedPartNode);
                if (nextPartBottomNode == null || nextPartBottomNode.attachedPart != curPart)
                {
                    Log.Info("nextPartBottomNode == null");
                    break;
                }
                if (nextPart.partName == "MSSRB_Part")
                {
                    if (!CheckPartResources(nextPart as MSSRB_Part, max))
                    {
                        Log.Info("CheckPartResources returns false");
                        break;
                    }
                }
                curPart = nextPart;
                attachNode = nextPart.FindAttachNode(node);
                if (attachNode == null)
                    break;
            }

            return totalFuelMass;
        }

        void ActivateEngine(string reason)
        {
            Log.Info("ActivateEngine");
            if (HighLogic.LoadedSceneIsFlight && !activated)
            {
                Log.Info("ActivateEngine, part: " + part.partInfo.title + ", reason: " + reason + ", activated: " + activated);
                double availAmt = 0;
                int cnt = 0;

                // Get total of all fuel in part

                availAmt = GetAllAvailableFuel(false);

                Log.Info("ActivateEngine: " + cnt++ + ", Part: " + part.partInfo.title + ", availAmt: " + availAmt + ", maxThrust: " + totalMaxThrust);

                part.Resources[ModSegSRBs.Propellant].amount =
                    part.Resources[ModSegSRBs.BurnablePropellant].amount = 0;
                maxThrust = totalMaxThrust / 2;
                bool vesselContainsBaseEngine = baseEngine != null && this.vessel.Parts.Contains(baseEngine.part);
                if (vesselContainsBaseEngine)
                {
                    baseEngine.SetBrokenSRB(totalMaxThrust, 0.5f, availAmt / 2);
                    baseEngine.part.Resources[ModSegSRBs.BurnablePropellant].amount =
                        part.Resources[ModSegSRBs.AbortedPropellant].maxAmount =
                            part.Resources[ModSegSRBs.AbortedPropellant].amount = availAmt / 2;
                }
                else
                {
                    part.Resources[ModSegSRBs.AbortedPropellant].maxAmount =
                        part.Resources[ModSegSRBs.AbortedPropellant].amount = availAmt;
                }

                Log.Info("ActivateEngine, availAmt: " + availAmt);
                enabled = true;
                activated = true;

                MSSRB_Engine.ChangeUsage(0.5f, ref this.maxThrust, ref this.atmosphereCurve);

                Log.Info("ActivateEngine, before baseEngine");
                if (vesselContainsBaseEngine)
                {
                    baseEngine.maxThrust = this.maxThrust / 2;
                    baseEngine.atmosphereCurve = this.atmosphereCurve;
                    this.maxFuelFlow = baseEngine.maxFuelFlow;
                    Log.Info("ActivateEngine, baseEngine.isActiveAndEnabled: " + baseEngine.isActiveAndEnabled);
                    Log.Info("ActivateEngine, baseEngine.part.Resources[ModSegSRBs.BurnablePropellant].amount: " + baseEngine.part.Resources[ModSegSRBs.BurnablePropellant].amount);
                    Log.Info("ActivateEngine, part.Resources[ModSegSRBs.AbortedPropellant].amount: " + part.Resources[ModSegSRBs.AbortedPropellant].amount);
                }

                Log.Info("maxThrust: " + maxThrust + ", maxFuelFlow: " + maxFuelFlow);
                base.Activate();

            }
        }

        internal void ActivateEngine2(float mt, float mff)
        {
            maxThrust = mt;
            maxFuelFlow = mff;
            Log.Info("ActivateEngine2, id: " + this.GetPersistentId());
            base.Activate();
        }

        public override void Activate()
        {
            // Makes sure that this isn't activated when the baseEngine is
            // Just leaves this as a do-nothing, otherwise the StageManager will activate this stage
        }

        new void FixedUpdate()
        {       
            // The base.FixedUpdate can get an exception if this part is destroyed, so
            // wrap it in a try/catch (I hate exceptions)
            try
            {
                base.FixedUpdate();
            } catch
            {
                return;
            }
            if (HighLogic.LoadedSceneIsEditor || this.vessel.situation <= Vessel.Situations.PRELAUNCH)
                return;

            if (HighLogic.LoadedSceneIsFlight && !activated)
            {
                if (baseEngine != null && baseEngine.EngineIgnited)
                {
                    if (attachNode != "" && attNode.attachedPart == null && !EngineIgnited)
                    {
                        ActivateEngine("attachedPart is null");
                    }
                    else
                    {
                        if (attachedSegment != null && thisSegment.part.segmentWidth > attachedSegment.part.segmentWidth)
                            ActivateEngine("This part wider than attached part");
                        if (attachedEndCap != null && thisSegment.part.segmentWidth > attachedEndCap.part.segmentWidth)
                            ActivateEngine("This part wider than attached part");
                        if (attachedMotor != null && thisSegment.part.segmentWidth > attachedMotor.part.segmentWidth)
                            ActivateEngine("This part wider than attached engine");
                    }
                }

                if (attachedSegment == null && attachedEndCap == null && attachedMotor == null)
                {
                    ActivateEngine("No valid attached part");
                }
            }
        }

        public new string GetModuleTitle()
        {
            return "";
        }

        public override string GetModuleDisplayName()
        {
            return "";
        }


        public override string GetInfo()
        {
            return "";
        }
    }
}
