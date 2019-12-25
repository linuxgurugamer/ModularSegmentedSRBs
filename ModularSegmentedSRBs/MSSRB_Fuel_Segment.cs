using KSP_Log;

namespace ModularSegmentedSRBs
{
    class MSSRB_Fuel_Segment : PartModule, IPartCostModifier
    {
        public new MSSRB_Part part { get { return base.part as MSSRB_Part; } }

        [KSPField]
        public string shape = "cylinder";

        internal MSSRB_Engine baseEngine;

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

        Log Log = new Log("ModularSegmentedSRBs.MSSRB_Fuel_Segment");

        internal double propAmt, burnablePropAmt;

        // Amount of fuel is directly dependent on the volume of the attached segments
        // for cylinder shape:   V = pi * r^2 * h
        // for circular truncated cone:  1/3 *pi * h (r1^2 + r1*r2  r2^2)
        public double MaxSolidFuel()
        {
            if (part == null)
                return 0;
            Log.Info("MaxSolidFuel, segment Width, Height: " + part.segmentWidth + ", " + part.segmentHeight);
            return ModSegSRBs.MaxSolidFuel(part.segmentWidth, part.segmentHeight);
        }

        public float GetMaxThrust()
        {
            Log.Info("GetMaxThrust, segment Width, Height: " + part.segmentWidth + ", " + part.segmentHeight);
            if (HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().BetterSRBsCompatibility)
                return ModSegSRBs.BetterSRBsThrustPerMeter * part.segmentHeight * part.segmentWidth;
            else
                return ModSegSRBs.ThrustPerMeter * part.segmentHeight * part.segmentWidth;
        }

        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            Log.Info("GetModuleCost, part.partInfo.title: " + this.part.partInfo.title + ", part.name: " + part.name);

            Log.Info("GetModuleCost, defaultCost: " + defaultCost);

            Log.Info("Propellant, amt/maxAmt: " + part.Resources[ModSegSRBs.Propellant].amount + "/" + part.Resources[ModSegSRBs.Propellant].maxAmount);
            Log.Info("BurnablePropellant, amt/maxAmt: " + part.Resources[ModSegSRBs.BurnablePropellant].amount + "/" + part.Resources[ModSegSRBs.BurnablePropellant].maxAmount);
            Log.Info("AbortedPropellant, amt/maxAmt: " + part.Resources[ModSegSRBs.AbortedPropellant].amount + "/" + part.Resources[ModSegSRBs.AbortedPropellant].maxAmount);
            if (part.partInstantiatedFlag == null)
            {

                //static internal void GetExtraInfo(PartVariant variant, ref float segmentHeight, ref float segmentWidth)
            }

            float rc;
            float fuelCost = 0;
            float segmentHeight = 0, segmentWidth = 0;
            if (part == null)
                rc = ModSegSRBs.GetSegmentCost(defaultCost, segmentWidth, segmentHeight, part, 0, ref fuelCost);
            else
                rc = ModSegSRBs.GetSegmentCost(defaultCost, part.segmentWidth, part.segmentHeight, part, (float)MaxSolidFuel(), ref fuelCost);
            Log.Info("GetModuleCost, rc: " + rc);
#if false
            if (part.partInstantiatedFlag == null)
            {
                Log.Info("this.part is null");
                rc -= fuelCost;
            }
#endif
            return rc;
        }

        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
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
            part.RestoreVariant();

            if (HighLogic.LoadedSceneIsEditor)
            {
                RecalculateFuelAndMass();

                GameEvents.onVariantApplied.Add(onEditorVariantApplied);
                if (baseEngine != null)
                    baseEngine.ScheduleSegmentUpdate("MSSRB_Fuel_Segment.Start");
            }
            else
            {
                propAmt = burnablePropAmt = 0;
                Events["TriggerSegmentFailureEvent"].active = HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().devMode;
                Actions["TriggerSegmentFailureAction"].active = HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().devMode;

            }

            if (part != null)
            {
                ModSegSRBs.GetExtraInfo(this.part.baseVariant, ref part.segmentHeight, ref part.segmentWidth);
            }

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
                burnableFuel.maxAmount = 0;
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
            if (part != base.part || part == null)
                return;

            if (variant == null || variant.DisplayName == null)
                return;
            ModSegSRBs.GetExtraInfo(variant, ref this.part.segmentHeight, ref this.part.segmentWidth);

            RecalculateFuelAndMass();
            var f = GetMaxThrust();

            if (baseEngine != null)
                baseEngine.ScheduleSegmentUpdate("MSSRB_Fuel_Segment.onEditorVariantApplied", 5);
#if true
            MonoUtilities.RefreshContextWindows(part);
#else
            MonoUtilities.RefreshPartContextWindow(part);
#endif
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
            return "MSSRB_Fuel_Segment title";
        }

        public override string GetModuleDisplayName()
        {
            return "MSSRB_Fuel_Segment";
        }
        public override string GetInfo()
        {
            return "Fuel segment";
        }
    }
}
