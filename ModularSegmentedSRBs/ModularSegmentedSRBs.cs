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
        public const float ThrustPerMeter = 36f; // gets multiplied by the width and length, this is roughly equivilent to the Kickback thrust/meter
        public const float BetterSRBsThrustPerMeter = 80f; // gets multiplied by the width and length, this is roughly equivilent to the Kickback thrust/meter
        public const float MassPerMeter = 0.03183101f;// gets multipled by Pi * width

        // Fuel Per Cubic Meter
        // Using the BAAC Thumper as reference
        //      1.25m x 1m segments       7
        //      Units                   820
        //      volume of 1 segment:    1.227184 cm
        //
        //                              (820 / 7) /1.228184 = 95.4566
        //
        public const float FuelPerCuM = 2 * 95.4566f;
        public const float pressurePulseHertz = 2;
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
