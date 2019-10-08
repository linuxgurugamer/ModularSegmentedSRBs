using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP_Log;

namespace ModularSegmentedSRBs
{
    class MS_SRB_Fuel_Segment : PartModule
    {
        [KSPField(isPersistant = true)]
        public float segmentHeight = 1.25f;

        [KSPField(isPersistant = true)]
        public float segmentWidth = 1.25f;

        [KSPField]
        public string shape = "cylinder";

        internal MS_SRB_Engine baseEngine;

        public float maxCalculatedThrust;

        bool triggerSegmentFailure = false;
        [KSPAction("Trigger segment failure")]
        public void TriggerSegmentFailureAction(KSPActionParam param)
        {
            triggerSegmentFailure = true;
        }
        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Trigger segment failure")]
        public void TriggerSegmentFailureEvent()
        {
            triggerSegmentFailure = true;
        }

        Log Log = new Log("ModularSegmentedSRBs.MS_SRB_Fuel_Segment");

        internal double propAmt, burnablePropAmt;

        // Amount of fuel is directly dependent on the volume of the attached segments
        // for cylinder shape:   V = pi * r^2 * h
        // for circular truncated cone:  1/3 *pi * h (r1^2 + r1*r2  r2^2)
        public double MaxSolidFuel()
        {
            Log.Info("MaxSolidFuel, width, height: " + segmentWidth + ", " + segmentHeight);
            Log.Info("MaxSolidFuel, FuelPerCuM: " + ModSegSRBs.FuelPerCuM + ", fuel: " + Math.PI * Math.Pow(segmentWidth / 2, 2) * segmentHeight * ModSegSRBs.FuelPerCuM);
            //   if (shape == "cylinder")
            return Math.PI * Math.Pow(segmentWidth / 2, 2) * segmentHeight * ModSegSRBs.FuelPerCuM;

#if false
            if (shape == "cone")
            {
                float height = segmentHeight / 2;
                float bottomRadius = segmentWidth / 2;
                float topRadius = segmentWidth / 4;

                double v = (Math.PI * height * (Math.Pow(bottomRadius, 2) + bottomRadius * topRadius + Math.Pow(topRadius, 2))) / 3;
                return segmentWidth / 2;
            }
            return 0;
#endif

        }

        public float GetSetMaxThrust
        {
            get
            {
                if (HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().BetterSRBsCompatibility)
                    maxCalculatedThrust = ModSegSRBs.BetterSRBsThrustPerMeter * segmentHeight * segmentWidth;
                else
                    maxCalculatedThrust = ModSegSRBs.ThrustPerMeter * segmentHeight * segmentWidth;
                return maxCalculatedThrust;
            }
        }

        public override void OnStart(StartState state)
        {
            Log.Info("OnStart");
            base.OnStart(state);
            Start();
        }

        public void Start()
        {
            Log.Info("Start");
            if (HighLogic.LoadedSceneIsEditor)
            {
                RecalculateFuelAndMass();

                GameEvents.onVariantApplied.Add(onEditorVariantApplied);
                if (baseEngine != null)
                    baseEngine.ScheduleSegmentUpdate("MS_SRB_Fuel_Segment.Start");
            }
            else
            {
                propAmt = burnablePropAmt = 0;
                Events["TriggerSegmentFailureEvent"].active = HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().devMode;
                Actions["TriggerSegmentFailureAction"].active = HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().devMode;
                
            }
            ModSegSRBs.GetExtraInfo(part.baseVariant, ref segmentHeight, ref segmentWidth);
        }


        void RecalculateFuelAndMass()
        {
            PartResource fuel = part.Resources[ModSegSRBs.Propellant];
            if (fuel != null)
            {
                fuel.amount =
                    fuel.maxAmount = MaxSolidFuel();
                Log.Info("RecalculateFuelAndMass, Propellant found: " + ModSegSRBs.Propellant + " fuel.maxAmount: " + fuel.maxAmount);
            }
            PartResource burnableFuel = part.Resources[ModSegSRBs.BurnablePropellant];
            if (burnableFuel != null)
            {
                burnableFuel.amount = 0;
                burnableFuel.maxAmount = MaxSolidFuel();
                Log.Info("RecalculateFuelAndMass, BurnablePropellant found: " + ModSegSRBs.BurnablePropellant + " fuel.maxAmount: " + fuel.maxAmount);
            }
            part.mass = (float)fuel.maxAmount * part.Resources[ModSegSRBs.Propellant].info.density;
        }

        void OnDestroy()
        {
            if (HighLogic.LoadedSceneIsEditor)
                GameEvents.onVariantApplied.Remove(onEditorVariantApplied);
        }


        private void onEditorVariantApplied(Part part, PartVariant variant)
        {
            // This should never be called in flight, but somehow it was, so just
            // have a check to be safe
            if (HighLogic.LoadedSceneIsFlight)
                return;
            if (part != base.part)
                return;

            if (variant == null || variant.DisplayName == null)
                return;
            ModSegSRBs.GetExtraInfo(variant, ref segmentHeight, ref segmentWidth);

            RecalculateFuelAndMass();
            var f = GetSetMaxThrust;

            //if (EditorLogic.fetch.ship !=null)
            //    GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            if (baseEngine != null)
                baseEngine.ScheduleSegmentUpdate("MS_SRB_Fuel_Segment.onEditorVariantApplied");
        }

        private void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                Log.Info("Part: " + part.partInfo.title + ", Propellant: " + part.Resources[ModSegSRBs.Propellant].amount +
                    ", Burnable: " + part.Resources[ModSegSRBs.BurnablePropellant].amount);
                Log.Info("Local, Propellant: " + propAmt + ", Burnable: " + burnablePropAmt);
                if (part.Resources[ModSegSRBs.Propellant].amount > 0)
                {
                    propAmt = part.Resources[ModSegSRBs.Propellant].amount;
                    burnablePropAmt = part.Resources[ModSegSRBs.BurnablePropellant].amount;
                }
                if (triggerSegmentFailure)
                {
                    this.part.explode();
                }

            }
        }

        public string GetModuleTitle()
        {
            return "MS_SRB_Fuel_Segment title";
        }

        public override string GetModuleDisplayName()
        {
            return "MS_SRB_Fuel_Segment";
        }
        public override string GetInfo()
        {
            return "Fuel segment";
        }
    }
}
