using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModularSegmentedSRBs
{
    public class ModSegSRBs
    {
        public static string SeparatronFuel = "SeparatronFuel";
        public static string Propellant = "SegmentedSolidFuel";
        public static string BurnablePropellant = "BurnableSegmentedSolidFuel";
        public static string AbortedPropellant = "AbortedSegmentedSolidFuel";

        public const float ThrustPerMeter = 36f; // gets multiplied by the width and length, this is roughly equivilent to the Kickback thrust/meter
        public const float BetterSRBsThrustPerMeter = 80f; // gets multiplied by the width and length, this is roughly equivilent to the Kickback thrust/meter
        public const float MassPerMeter = 0.03183101f;// gets multipled by Pi * width

        // Fuel Per Cubic Meter
        public static float FuelPerCuM { get { return 1 / GetDensity(Propellant); } }

        //public const float pressurePulseHertz = 7;

        static internal void  GetExtraInfo(PartVariant variant, ref float segmentHeight, ref float segmentWidth)
        {
            string strSegmentHeight = variant.GetExtraInfoValue("segmentHeight");
            string strSegmentWidth = variant.GetExtraInfoValue("segmentWidth");
            if (string.IsNullOrEmpty(strSegmentHeight) || string.IsNullOrEmpty(strSegmentWidth))
            {
                return;
            }
            segmentHeight = float.Parse(strSegmentHeight);
            segmentWidth = float.Parse(strSegmentWidth);
        }

        static float GetDensity(string propellant)
        {
            PartResourceDefinition prd = PartResourceLibrary.Instance.GetDefinition(propellant);
            return prd.density;
        }
    }
}
/*
 * 
fFuelFlowMass += (p.RequestResource(resourceName, (fMassFlow / mixtureDensity) * TimeWarp.fixedDeltaTime)) * mixtureDensity;
    
15.827     227     14.342
 15.821     192     12.135

    	PROPELLANT
		{
			name = SegmentedSolidFuelEditor
			ratio = 1.0
			DrawGauge = True
		}

 */
