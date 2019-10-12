﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP_Log;

namespace ModularSegmentedSRBs
{
    class MS_SRB_SegmentEnds : ModuleEnginesFX
    {
        internal MS_SRB_Engine baseEngine = null; // This is set in the MS_SRB_Engine module

#if true
        [KSPField]
        public string attachNode = null;

#if false
        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Test Manual Activate")]
        public void DoActivate()
        {

            ScreenMessages.PostScreenMessage("Activate", 5);
            ActivateEngine("Manual activate");
            //ActivateEngine("DoActivate");
            return;

            enabled = true;

            part.Resources[ModSegSRBs.BurnablePropellant].maxAmount =
                                part.Resources[ModSegSRBs.BurnablePropellant].amount = part.Resources[ModSegSRBs.Propellant].amount + part.Resources[ModSegSRBs.BurnablePropellant].amount;
            part.Resources[ModSegSRBs.Propellant].amount = 0;

            base.Activate();
        }
#endif

        public override bool IsStageable() { return false; }



        AttachNode attNode;
        MS_SRB_Fuel_Segment thisSegment;
        MS_SRB_Fuel_Segment attachedSegment = null;
        MS_SRB_Endcap attachedEndCap = null;
        MS_SRB_Engine attachedMotor = null;

        Log Log = new Log("ModularSegmentedSRBs.MS_SRB_SegmentEnds");

        void Start()
        {
            nonThrustMotor = true;
                attNode = this.part.FindAttachNode(attachNode);
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
                if (pml.Contains<MS_SRB_Fuel_Segment>())
                    thisSegment = pml.GetModule<MS_SRB_Fuel_Segment>();

                if (attNode != null && attNode.attachedPart != null)
                {
                    pml = attNode.attachedPart.Modules;
                    if (pml != null)
                    {
                        if (pml.Contains<MS_SRB_Fuel_Segment>())
                            attachedSegment = pml.GetModule<MS_SRB_Fuel_Segment>();

                        if (pml.Contains<MS_SRB_Endcap>())
                            attachedEndCap = pml.GetModule<MS_SRB_Endcap>();

                        if (pml.Contains<MS_SRB_Engine>())
                            attachedMotor = pml.GetModule<MS_SRB_Engine>();
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

        double totalFuelMass;
        float totalMaxThrust;
        bool CheckPartResources(Part part)
        {
            // Check the nextPart's resources and Modules
            // Check for the correct propellant and that it has the MS_SRB_Fuel_Segment module
            // If so, add it to the segments list and accumulate some values for the motor
            // then update the propellant value amount in the fuel segment
            PartResourceList prl = part.Resources;
            PartModuleList pml = part.Modules;
            if (prl.Contains(ModSegSRBs.Propellant) && pml.Contains<MS_SRB_Fuel_Segment>())
            {
                Log.Info("CheckPartResources, " + ModSegSRBs.Propellant + " found, " + " MS_SRB_Fuel_Segment found");
                MS_SRB_Fuel_Segment moduleFuelSegment = pml["MS_SRB_Fuel_Segment"] as MS_SRB_Fuel_Segment;

              
                totalFuelMass += moduleFuelSegment.MaxSolidFuel();  // prl[ModSegSRBs.Propellant].maxAmount;                
                totalMaxThrust += moduleFuelSegment.GetSetMaxThrust;


                //totalSolidFuel += moduleFuelSegment.MaxSolidFuel();
                //segmentWidth = Math.Max(segmentWidth, moduleFuelSegment.segmentWidth);

   
  
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

        double GetAllAvailableFuel()
        {
            string node = "top", attachedPartNode = "bottom" ;
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
            CheckPartResources(this.part);
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
                if (!CheckPartResources(nextPart))
                {
                    Log.Info("CheckPartResources returns false");
                    break;
                }
                curPart = nextPart;
                attachNode = nextPart.FindAttachNode(node);
                if (attachNode == null)
                    break;
            }

            return totalFuelMass;
        }

        bool activated = false;
        void ActivateEngine(string reason)
        {
            Log.Info("ActivateEngine");
            if (HighLogic.LoadedSceneIsFlight && !activated)
            {
                if (part == null)
                    Log.Info("ActivateEngine, part is null");
                if (reason == null)
                    Log.Info("ActivateEngine, reason is null");
                Log.Info("ActivateEngine, part: " + part.partInfo.title + ", reason: " + reason + ", activated: " + activated);
               double availAmt = 0;
                int cnt = 0;

                // Get total of all fuel in part

                availAmt = GetAllAvailableFuel();

                Log.Info("ActivateEngine: " + cnt++ + ", Part: " + part.partInfo.title + ", availAmt: " + availAmt + ", maxThrust: " + totalMaxThrust);

                part.Resources[ModSegSRBs.Propellant].amount = part.Resources[ModSegSRBs.BurnablePropellant].amount = 0;
                maxThrust = totalMaxThrust /2;
                if (baseEngine != null)
                {
                    baseEngine.SetBrokenSRB(totalMaxThrust, 0.5f, availAmt / 2);
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
#if false
                //if (baseEngine.part != this.part)
                {
                    //double half =
                    part.Resources[ModSegSRBs.AbortedPropellant].maxAmount =
                                   part.Resources[ModSegSRBs.AbortedPropellant].amount = availAmt;

                    part.Resources[ModSegSRBs.BurnablePropellant].maxAmount =
                                   part.Resources[ModSegSRBs.BurnablePropellant].amount = 0;
                    //part.Resources[ModSegSRBs.Propellant].maxAmount;

                    part.Resources[ModSegSRBs.Propellant].amount = 0;
                    //Log.Info("maxAmount: " + part.Resources[ModSegSRBs.BurnablePropellant].maxAmount);
                    maxThrust = 75;
                    maxFuelFlow = 0.01f;
                }
#endif
                MS_SRB_Engine.ChangeUsage(0.5f, ref this.maxThrust, ref this.atmosphereCurve);
                if (baseEngine != null)
                {
                    baseEngine.maxThrust = this.maxThrust/ 2;
                    baseEngine.atmosphereCurve = this.atmosphereCurve;
                    this.maxFuelFlow = baseEngine.maxFuelFlow;
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

            //ScreenMessages.PostScreenMessage("Activate", 5);
            //ActivateEngine("Manual activate");
        }

        new void FixedUpdate()
        {
            base.FixedUpdate();
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
                        if (attachedSegment != null && thisSegment.segmentWidth > attachedSegment.segmentWidth)
                            ActivateEngine("This part wider than attached part");

                        if (attachedEndCap != null && thisSegment.segmentWidth > attachedEndCap.segmentWidth)
                            ActivateEngine("This part wider than attached part");
                        if (attachedMotor != null && thisSegment.segmentWidth > attachedMotor.segmentWidth)
                            ActivateEngine("This part wider than attached engine");
                    }
                }

                if (attachedSegment == null && attachedEndCap == null && attachedMotor == null)
                {
                    ActivateEngine("No valid attached part");
                }
            }
        }
#endif
    }
}
