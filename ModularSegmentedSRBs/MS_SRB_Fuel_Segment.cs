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


#if true
        Log Log = new Log("ModularSegmentedSRBs.MS_SRB_Fuel_Segment");
#endif
        // Amount of fuel is directly dependent on the volume of the attached segments
        // for cylinder shape:   V = pi * r^2 * h
        // for circular truncated cone:  1/3 *pi * h (r1^2 + r1*r2  r2^2)
        public double MaxSolidFuel()
        {
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

        public void Start()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                RecalculateFuelAndMass();

                GameEvents.onVariantApplied.Add(onEditorVariantApplied);
            }
            else
            {

                if (!HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().devMode)
                {
                    Events["TriggerSegmentFailureEvent"].active = false;
                    Actions["TriggerSegmentFailureAction"].active = false;
                }
            }
        }

        
        void RecalculateFuelAndMass()
        {
            PartResource fuel = part.Resources[ModSegSRBs.Propellant];
            if (fuel != null)
            {
                fuel.amount = 
                    fuel.maxAmount = MaxSolidFuel();
                
            }
            PartResource burnableFuel = part.Resources[ModSegSRBs.BurnablePropellant];
            if (burnableFuel != null)
            {
                burnableFuel.amount = 0;
                burnableFuel.maxAmount = MaxSolidFuel();
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
            if (part == null || part != base.part)
            {
                return;
            }
            string strSegmentHeight = variant.GetExtraInfoValue("segmentHeight");
            string strSegmentWidth = variant.GetExtraInfoValue("segmentWidth");
            if (string.IsNullOrEmpty(strSegmentHeight))
            {
                strSegmentHeight = "2";
            }
            if (string.IsNullOrEmpty(strSegmentWidth))
            {
                strSegmentWidth = "1.25";
            }
            segmentHeight = float.Parse(strSegmentHeight);
            segmentWidth = float.Parse(strSegmentWidth);

            RecalculateFuelAndMass();
            var f = GetSetMaxThrust;

            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
        }

        private void FixedUpdate()
        {
            if (triggerSegmentFailure)
            {
                this.part.explode();
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
