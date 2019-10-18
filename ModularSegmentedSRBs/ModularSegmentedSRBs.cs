using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP_Log;
using UnityEngine;

namespace ModularSegmentedSRBs
{
    public class ModSegSRBs
    {
        public static string SeparatronFuel = "SeparatronFuel";
        public static string Propellant = "SegmentedSolidFuel";
        public static string BurnablePropellant = "BurnableSegmentedSolidFuel";
        public static string AbortedPropellant = "AbortedSegmentedSolidFuel";

        public const string ParachuteTechName = "MSSRB.Parachute";
        public const string MFCTechName = "MSSRB.FlightControl";
        public const string PPTechName = "MSSRB.PrecisionPropulsion";

        public const float ThrustPerMeter = 36f; // gets multiplied by the width and length, this is roughly equivilent to the Kickback thrust/meter
        public const float BetterSRBsThrustPerMeter = 80f; // gets multiplied by the width and length, this is roughly equivilent to the Kickback thrust/meter
        public const float MassPerMeter = 0.03183101f;// gets multipled by Pi * width

        // Fuel Per Cubic Meter
        public static float FuelPerCuM { get { return 1 / GetDensity(Propellant); } }

#if THRUSTVARIABILITY
        public const float pressurePulseHertz = 7;
#endif

        static internal void GetExtraInfo(PartVariant variant, ref float segmentHeight, ref float segmentWidth)
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

        // Amount of fuel is directly dependent on the volume of the attached segments
        // for cylinder shape:   V = pi * r^2 * h
        // for circular truncated cone:  1/3 *pi * h (r1^2 + r1*r2  r2^2)
        static public double MaxSolidFuel(float segmentWidth, float segmentHeight)
        {
            Debug.Log("MaxSolidFuel, segment Width, Height: " + segmentWidth + "x" + segmentHeight);
            if (segmentWidth == 0 || segmentHeight == 0)
                return 0;
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

        static float GetDensity(string propellant)
        {
            PartResourceDefinition prd = PartResourceLibrary.Instance.GetDefinition(propellant);
            return prd.density;
        }

        static internal float GetSegmentCost(float defaultCost, float segmentWidth, float segmentHeight, Part part, float maxSolidFuel, ref float fuelCost)
        {
            if (segmentWidth == 0 || segmentHeight == 0)
                return 0;
            // Get the surface area of the cylinder walls.  Do NOT include the ends here
            float cylSurfaceArea = (float)(Math.PI * segmentWidth * segmentHeight);
            float tankCost = (float)Math.Ceiling(cylSurfaceArea * 12f * (float)Math.Log(segmentWidth + 1));
            //return tankCost; // +  defaultCost;

            PartResource fuel = part.Resources[ModSegSRBs.Propellant];
            fuelCost = maxSolidFuel * fuel.info.unitCost;

            Debug.Log("GetSegmentCost, width, height: " + segmentWidth + "x" + segmentHeight + ", cylSurfaceArea: " + cylSurfaceArea
                + ", tankCost: " + tankCost + ", fuelCost(" + maxSolidFuel + "): " + fuelCost);

            return tankCost + fuelCost; // /* + fuelCost */ - defaultCost;
        }


        public static bool PartAvailable(string TechName)
        {
            AvailablePart techPart;
            bool techPartResearched;
            if (PartLoader.DoesPartExist(TechName))
            {
                techPart = PartLoader.getPartInfoByName(TechName);
                techPartResearched = PartResearched(techPart);
                return techPartResearched;
            }
            return false;
        }
        static bool PartResearched(AvailablePart p)
        {
            if (p == null)
                return false;
            return ResearchAndDevelopment.PartTechAvailable(p) && ResearchAndDevelopment.PartModelPurchased(p);
        }
    }
}