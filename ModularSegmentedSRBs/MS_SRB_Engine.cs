using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP.Localization;
using UnityEngine;
using KSP_Log;
using KSP_PartHighlighter;

namespace ModularSegmentedSRBs
{
    internal class Segment
    {
        internal Part part;
        internal double ratio;
        internal float segmentHeight;

        public Segment(Part p, float segHeight)
        {
            part = p;
            ratio = 0;

            segmentHeight = segHeight;
        }
    }

    public class MS_SRB_Engine : ModuleEnginesFX
    {
        Log Log = new Log("ModularSegmentedSRBs.MS_SRB_Engine");

        public float baseHeatProduction = 545;

        List<Segment> segments = new List<Segment>();

        float totalSegmentHeight = 0;
        float invTotalSegmentHeight = 0;
        double totalMaxThrust = 0;
        double totalFuelMass = 0;
        double totalFuelFlow = 0f;
        //float segmentWidth = 1.25f;
        // double totalSolidFuel = 0;
        float failureChance = 0;

        [KSPField(isPersistant = true)]
        public float segmentHeight = 1.25f;

        [KSPField(isPersistant = true)]
        public float segmentWidth = 1.25f;

        [KSPField]
        public float thrustModifier = 1;

        [KSPField(guiFormat = "F5", guiActive = true, guiName = "Fuel Density", guiActiveEditor = true)]
        float density = 0f;

        bool triggerEngineFailure = false;
        [KSPAction("Trigger engine failure")]
        public void TriggerEngineFailureAction(KSPActionParam param)
        {
            triggerEngineFailure = true;
        }

        [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Trigger engine failure")]
        public void TriggerEngineFailureEvent()
        {
            triggerEngineFailure = true;
        }

        int updateSegmentsIn = -1;

        public void ScheduleSegmentUpdate(string s, int i = 3)
        {
            updateSegmentsIn = i;
            Log.Info("ScheduleSegmentUpdate, from: " + s);
        }

        // Following are for run-time efficiency
        public double densityInverse;
        public double fuelFlowMultiplier = 2.66666f;

        internal PartHighlighter phl = null;
        internal int highlightID = -1;

        FloatCurve origAtmosphereCurve;
        float origMaxThrust;


        float fEngRunTime = 0f;

        float engineHealth = 1f;


        float oldEngineHealth = 1f;
        bool sickEngine = false;
        internal bool brokenSRB = false;
        float explode = -1;

        float percentLeft = 1;

        EditorMarker_CoM CoM, oCoM;
        int comCheckCntr = 0;
        int burnablePropIndx = 0;

        static FloatCurve ChangeFloatCurve(float changeMult, FloatCurve atmosphereCurve)
        {
            FloatCurve newAtmoCurve = new FloatCurve();
            // First save to a config node.
            ConfigNode node = new ConfigNode();
            atmosphereCurve.Save(node);

            var keys = node.GetValuesList("key");
            foreach (var s in keys)
            {
                var values = s.Split(' ').ToList();
                newAtmoCurve.Add(float.Parse(values[0]), float.Parse(values[1]) * changeMult);
            }
            return newAtmoCurve;
        }

        static float ChangeThrust(float changeMult, float maxThrust)
        {
            return maxThrust * changeMult;
        }

        internal void ChangeUsage(float changeMult) //, float thrustVariability = 0)
        {
            atmosphereCurve = origAtmosphereCurve;
            maxThrust = origMaxThrust;
            ChangeUsage(changeMult, ref maxThrust, ref atmosphereCurve);            
        }

        internal static void ChangeUsage(float changeMult, ref float maxThrust, ref FloatCurve atmosphereCurve) //, float thrustVariability = 0)
        {
            maxThrust = ChangeThrust(changeMult, maxThrust);
            atmosphereCurve = ChangeFloatCurve(changeMult, atmosphereCurve);
        }

        internal void SetEngineHealth(float changeMult)
        {
            engineHealth = changeMult;
        }

        private void Start()
        {
            Events["TriggerEngineFailureEvent"].active = HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().devMode;
            Actions["TriggerEngineFailureAction"].active = HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().devMode;

            foreach (var e in Events)
                Log.Info("Event: " + e.name + ", guiName = " + e.GUIName);
            foreach (var a in Actions)
                Log.Info("Action: " + a.name + ", guiName: " + a.guiName);

            Actions["ToggleThrottle"].active =
                Actions["ToggleThrottle"].activeEditor = false;
            origAtmosphereCurve = atmosphereCurve;

            PartResourceDefinition prd = PartResourceLibrary.Instance.GetDefinition(ModSegSRBs.Propellant);
            density = prd.density;
            densityInverse = 1.0 / prd.density;
            fuelFlowMultiplier = densityInverse * Planetarium.fetch.fixedDeltaTime;

            phl = PartHighlighter.CreatePartHighlighter();
            if (phl)
                highlightID = phl.CreateHighlightList();
            if (highlightID >= 0)
                UpdateHighlightColors();

            foreach (var p in propellants)
            {
                if (p.name == ModSegSRBs.BurnablePropellant)
                {
                    break;
                }
                burnablePropIndx++;
            }
            ModSegSRBs.GetExtraInfo(part.baseVariant, ref segmentHeight, ref segmentWidth);

            switch (HighLogic.LoadedScene)
            {
                case GameScenes.EDITOR:
                    //GameEvents.onEditorShipModified.Add(onEditorShipModified);

                    //EventData<ConstructionEventType, Part> onEditorPartEvent;
                    GameEvents.onEditorPartPicked.Add(onEditorPartPicked);
                    GameEvents.onEditorPartPlaced.Add(onEditorPartPlaced);
                    GameEvents.onEditorPartDeleted.Add(onEditorPartDeleted);
                    GameEvents.onVariantApplied.Add(onEditorVariantApplied);
                    ScheduleSegmentUpdate("Start");
                    break;

                case GameScenes.FLIGHT:
                    failureChance = HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().failureChance;
                    if (this.vessel.situation <= Vessel.Situations.PRELAUNCH)
                    {
                        UpdateSegments();
                        foreach (var s in segments)
                        {
                            PartModuleList pml = s.part.Modules;
                            MS_SRB_Fuel_Segment moduleFuelSegment = pml["MS_SRB_Fuel_Segment"] as MS_SRB_Fuel_Segment;

                            PartResourceList prl = s.part.Resources;
                            if (prl.Contains(ModSegSRBs.Propellant))
                            {
                                //Log.Info("Segment: " + s.part.partInfo.title + " contains Propellant");
                                prl[ModSegSRBs.Propellant].amount = moduleFuelSegment.MaxSolidFuel();
                            }
                            if (prl.Contains(ModSegSRBs.BurnablePropellant))
                            {
                                prl[ModSegSRBs.BurnablePropellant].amount = 0;
                                //prl[ModSegSRBs.BurnablePropellant].amount = moduleFuelSegment.MaxSolidFuel();

                                //Log.Info("Segment: " + s.part.partInfo.title + " contains BurnablePropellant");
                            }
                        }
                    }
                    break;

            } // switch
        }


        void OnDestroy()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorPartPicked.Remove(onEditorPartPicked);
                GameEvents.onEditorPartPlaced.Remove(onEditorPartPlaced);
                GameEvents.onEditorPartDeleted.Remove(onEditorPartDeleted);
                GameEvents.onEditorShipModified.Remove(onEditorShipModified);
                GameEvents.onVariantApplied.Remove(onEditorVariantApplied);
            }
        }

        private void onEditorPartPlaced(Part p) { ScheduleSegmentUpdate("onEditorPartPlaced"); }

        private void onEditorPartPicked(Part p) { ScheduleSegmentUpdate("onEditorPartPlaced"); }

        private void onEditorPartDeleted(Part p) { ScheduleSegmentUpdate("onEditorPartDeleted"); }

        private void onEditorVariantApplied(Part part, PartVariant variant)
        {
            if (part != base.part)
                return;
            ModSegSRBs.GetExtraInfo(variant, ref segmentHeight, ref segmentWidth);
            ScheduleSegmentUpdate("onEditorVariantApplied");
        }

        void onEditorShipModified(ShipConstruct construct) { ScheduleSegmentUpdate("onEditorShipModified"); }

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

                segments.Add(new Segment(part, moduleFuelSegment.segmentHeight));
                totalFuelMass += moduleFuelSegment.MaxSolidFuel();  // prl[ModSegSRBs.Propellant].maxAmount;


                totalMaxThrust += moduleFuelSegment.GetSetMaxThrust;
                totalSegmentHeight += moduleFuelSegment.segmentHeight;
                if (totalSegmentHeight > 0)
                    invTotalSegmentHeight = 1 / totalSegmentHeight;
                else
                    invTotalSegmentHeight = 0;

                //totalSolidFuel += moduleFuelSegment.MaxSolidFuel();
                //segmentWidth = Math.Max(segmentWidth, moduleFuelSegment.segmentWidth);

                moduleFuelSegment.baseEngine = this;

                if (CoM == null && HighLogic.LoadedSceneIsEditor)
                {
                    prl[ModSegSRBs.Propellant].amount = 0;
                    prl[ModSegSRBs.BurnablePropellant].amount = 0; // moduleFuelSegment.MaxSolidFuel();
                    Log.Info("(2) Propellant Setting part.amount = 0, BurnablePropellant maxAmount set to: " + prl[ModSegSRBs.BurnablePropellant].maxAmount);
                }
                else
                {
                    prl[ModSegSRBs.Propellant].amount = moduleFuelSegment.MaxSolidFuel();
                    prl[ModSegSRBs.BurnablePropellant].amount = 0;
                }
                prl[ModSegSRBs.Propellant].maxAmount =
                    prl[ModSegSRBs.BurnablePropellant].maxAmount = moduleFuelSegment.MaxSolidFuel();
                Log.Info("totalMaxThrust: " + totalMaxThrust + ", totalFuelMass: " + totalFuelMass);
                // Set the baseEngine in the segment ends here, do it in a loop
                // because there can be either one or two ends
                foreach (var p in pml)
                {
                    if (p is MS_SRB_SegmentEnds)
                    {
                        MS_SRB_SegmentEnds mssrb = p as MS_SRB_SegmentEnds;
                        mssrb.baseEngine = this;
                        mssrb.maxThrust = this.maxThrust;
                        mssrb.atmosphereCurve = this.atmosphereCurve;
                    }
                }

                return true;

            }
            else
            {
                Log.Info("CheckPartResources, no Propellant found");
                return false;
            }
        }

        void UpdateSegments()
        {
            if (part == null)
            {
                Log.Info("UpdateSegments, part is null");
                return;
            }
            Log.Info("UpdateSegments, updateSegmentsIn: " + updateSegmentsIn);

            // Get a list of all the attached fuel segments, following the nodes from the top node
            // of the engine to the bottom node of the attached segment, and then the top node of the segment, etc
            // Stop when a part does not have the correct fuel resource

            totalMaxThrust = 0;
            totalSegmentHeight = 0;
            //segmentWidth = 0;
            totalFuelMass = 0;
            //totalSolidFuel = 0;
            segments.Clear();
            Part curPart = this.part;
            Part nextPart;

            AttachNode topAttachNode = this.part.FindAttachNode("top");
            //if (topAttachNode == null)
            //    return;
            /* bool motorHasFuel = */
            CheckPartResources(this.part);

            while (topAttachNode.attachedPart != null)
            {
                nextPart = topAttachNode.attachedPart;

                // Make sure that the part attached to this part's topNode is that part's bottom node
                AttachNode nextPartBottomNode = nextPart.FindAttachNode("bottom");
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
                topAttachNode = nextPart.FindAttachNode("top");
                if (topAttachNode == null)
                    break;
            }


            //foreach (var s in segments)
            for (int i = 0; i < segments.Count; i++)
            {
                Segment s = segments[i];
                PartResourceList prl = s.part.Resources;
                s.ratio = prl[ModSegSRBs.Propellant].maxAmount / totalFuelMass;
                Log.Info("UpdateSegments, segment[" + i + "]: " + s.part.partInfo.title + ", resources: " + prl[ModSegSRBs.Propellant].maxAmount);
            }
            Log.Info("UpdateSegments, totalFuelMass: " + totalFuelMass);

            maxThrust = (float)totalMaxThrust * thrustModifier;
            maxFuelFlow = (float) /*Planetarium.fetch.fixedDeltaTime* */(maxThrust /*  * mixtureDensityRecip  */) / (atmosphereCurve.Evaluate(0f) * g);
            origMaxThrust = maxThrust;

            PartResource pr = part.Resources[ModSegSRBs.BurnablePropellant];

            if (CoM != null || HighLogic.LoadedSceneIsFlight)
            {
                Log.Info("(1) BurnablePropellant Setting part.amount = 0");
                pr.amount = 0;
                pr.maxAmount = maxFuelFlow;
            }
            else
            {
                Log.Info("(1) BurnablePropellant Setting part.amount = " + totalFuelMass);
                pr.amount = totalFuelMass;
                pr.maxAmount = totalFuelMass;
            }

            if (segmentWidth > 0)
            {
                float lengthToWidthRatio = totalSegmentHeight / segmentWidth;
                heatProduction = baseHeatProduction + 5 * lengthToWidthRatio;

                if (lengthToWidthRatio > HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().maxSafeWidthToLengthRatio)
                {
                    float excess = totalSegmentHeight - segmentWidth * HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().maxSafeWidthToLengthRatio;
                    failureChance = Math.Max(failureChance, excess * invTotalSegmentHeight / (50 * 60));
                    if (HighLogic.LoadedSceneIsEditor)
                    {
                        if (HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().warnOnExcessiveHeight)
                            ScreenMessages.PostScreenMessage("* Warning *  SRB length exceeds safe limits by " + (100 * excess * invTotalSegmentHeight).ToString("F1") + "%", 5);

                        HighlightStack();
                    }
                }
                else
                {
                    UnHighlightStack();
                    failureChance = HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().failureChance;
                }
            }
            else
                heatProduction = 0;

            totalFuelFlow = maxFuelFlow / Planetarium.fetch.fixedDeltaTime;


            GameEvents.onChangeEngineDVIncludeState.Fire(this);
        }
#if false
        internal void SetBrokenSRB(float percentage, double abortFuelAmt)
        {
            Log.Info("SetBrokenSRB, abortFuelAmt: " + abortFuelAmt);
            ChangeUsage(percentage, ref maxThrust, ref atmosphereCurve);
            brokenSRB = true;

            part.Resources[ModSegSRBs.AbortedPropellant].maxAmount =
                                  part.Resources[ModSegSRBs.AbortedPropellant].amount = abortFuelAmt;
            part.Resources[ModSegSRBs.Propellant].amount = part.Resources[ModSegSRBs.BurnablePropellant].amount = 0;
            part.Resources[ModSegSRBs.BurnablePropellant].maxAmount =
                                  part.Resources[ModSegSRBs.BurnablePropellant].maxAmount = 0;

            List<MS_SRB_SegmentEnds> smfLst = part.Modules.GetModules<MS_SRB_SegmentEnds>();
            foreach (var se in smfLst)
            {
                // if (se.attachNode == null || se.attachNode == "")
                {
                    Log.Info("SetBrokenSRB, activating");
                    se.ActivateEngine2(maxThrust, maxFuelFlow);
                }

            }
        }
#endif

        void UpdateSegmentsInFlight()
        {
            if (this.EngineIgnited && !brokenSRB)
            {
#if true // for testing
                if (failureChance > 0)
                {
                    if (!sickEngine && (UnityEngine.Random.Range(0f, 1f) <= failureChance || triggerEngineFailure))
                    {
                        ScreenMessages.PostScreenMessage("***** Warning ***** Warning ***** Warning *****", 5);
                        ScreenMessages.PostScreenMessage("***** SRB failure in progress *****", 5);

                        HighlightStack();
                        AudibleAlertCore.fetch.ActivateAlarm(5);

                        if (UnityEngine.Random.Range(0, 1f) <= HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().explosionChance)
                        {
                            // Part will explode in 5 seconds
                            explode = fEngRunTime;
                        }
                        else
                        {
                            // Start reducing thrust
                            sickEngine = true;
                            heatProduction = 0;
                        }
                    }
                }
                if (explode > 0 && fEngRunTime - explode > 5)
                {
                    // Pick a part and destroy it
                    int i = UnityEngine.Random.Range(0, segments.Count - 1);
                    segments[i].part.explode();
                }
                if (sickEngine)
                    engineHealth = Math.Max(0.1f, engineHealth - UnityEngine.Random.Range(0, 0.01f)); 

                if (engineHealth < oldEngineHealth) // || HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().hasThrustVariability)
                {
                    oldEngineHealth = engineHealth;
#if false
                    if (HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().hasThrustVariability)
                    {
                        fEngRunTime += TimeWarp.fixedDeltaTime;
                        ChangeUsage(engineHealth, 0.5f * Mathf.Sin(fEngRunTime * Mathf.PI * 2 * ModSegSRBs.pressurePulseHertz));
                    } else
#endif
                    {
                        ChangeUsage(engineHealth);
                    }
                }
#endif

                PartResource secondPartResource = null;

                totalFuelFlow = 0;

                double totalCurrentFuel = 0;
                double fuelFlow = 0;
                double totalMaxFuel = 0;
                var maxFuelFlow = getMaxFuelFlow(propellants[burnablePropIndx]) * Time.fixedDeltaTime - part.Resources[ModSegSRBs.BurnablePropellant].amount + 0.05f;


                for (int i = 0; i < segments.Count; i++)
                {
                    Segment segment = segments[i];
                    MS_SRB_Fuel_Segment moduleFuelSegment = segment.part.Modules["MS_SRB_Fuel_Segment"] as MS_SRB_Fuel_Segment;

                    if (segment.segmentHeight > 0)
                    {
                        fuelFlow = segment.segmentHeight * invTotalSegmentHeight * maxFuelFlow;

                        fuelFlow *= engineHealth;
                        maxFuelFlow *= engineHealth;

                        PartResourceList prl = segment.part.Resources;
                        double availableResource = Math.Min(fuelFlow, prl[ModSegSRBs.Propellant].amount);
                        prl[ModSegSRBs.Propellant].amount -= availableResource;
                        totalFuelFlow += availableResource;
                        totalCurrentFuel += prl[ModSegSRBs.Propellant].amount;
                        totalMaxFuel += prl[ModSegSRBs.Propellant].maxAmount;

                        if (((MS_SRB_SegmentEnds)segment.part.Modules["MS_SRB_SegmentEnds"]).EngineIgnited)
                        {
                            secondPartResource = segment.part.Resources[ModSegSRBs.BurnablePropellant];
                        }
                        else
                            secondPartResource = null;
                    }
                }
                return;
                totalFuelFlow *= Time.timeScale;
                if (totalFuelFlow > 0)
                {
                    if (secondPartResource != null)
                    {
                        double t = (secondPartResource.amount + totalFuelFlow + part.Resources[ModSegSRBs.BurnablePropellant].amount) / 2;
                        secondPartResource.amount =
                            part.Resources[ModSegSRBs.BurnablePropellant].amount = t;

                    }
                    else
                    {
                        // Update resources in the engine, and then update
                        // the maxAmount with the inverse of the percentage of fuel remaining
                        // This fools the game into showing the correct amount in the staging toolbar
                        part.Resources[ModSegSRBs.BurnablePropellant].amount += totalFuelFlow;


                    }
                    // Now update the maxAmount with the inverse of the percentage of fuel remaining
                    // This fools the game into showing the correct amount in the staging toolbar

                    part.Resources[ModSegSRBs.BurnablePropellant].maxAmount =
                        part.Resources[ModSegSRBs.BurnablePropellant].amount * totalFuelMass / (part.Resources[ModSegSRBs.BurnablePropellant].amount + totalCurrentFuel);
                    percentLeft = (float)(totalCurrentFuel / totalMaxFuel);
                }
            }
        }

        void UpdateSegmentsInEditor()
        {

            comCheckCntr++;
            if (comCheckCntr > 15)
            {
                comCheckCntr = 0;

                oCoM = CoM;
                CoM = (EditorMarker_CoM)FindObjectOfType(typeof(EditorMarker_CoM));
                if (CoM != oCoM)
                {
                    UpdateSegments();
                    updateSegmentsIn = -1;
                }
            }
            if (updateSegmentsIn-- == 0)
                UpdateSegments();
        }


        new void FixedUpdate()
        {
            switch (HighLogic.LoadedScene)
            {
                case GameScenes.FLIGHT:
                    UpdateSegmentsInFlight();
                    break;
                case GameScenes.EDITOR:
                    UpdateSegmentsInEditor();
                    break;
            }

            base.FixedUpdate();
        }

        public new string GetModuleTitle()
        {
            return "MS_SRB_Engine";
        }

        public override string GetModuleDisplayName()
        {
            return "MS_SRB_Engine";
        }

        protected override string GetInfoThrust(bool mainInfoWindow)
        {
            string str = string.Empty;

            str += "<b>Max.Thrust</b> depends on number and height of segments\n";
            str += "<color=orange>Only uses directly attached segments.</color>\n";
            return str;
        }

        public override string GetInfo()
        {
            string infoThrust = GetInfoThrust(true);
            string str = infoThrust;
            string[] obj = new string[2];
            float num = atmosphereCurve.Evaluate(1f);
            obj[0] = num.ToString("0.###");
            float num2 = atmosphereCurve.Evaluate(0f);
            obj[1] = num2.ToString("0.###");
            infoThrust = str + Localizer.Format("#autoLOC_220745", obj); // isp
            infoThrust += Localizer.Format("#autoLOC_220748"); // propellants

            Propellant propellant = propellants[burnablePropIndx];
            string text = KSPUtil.PrintModuleName(propellant.displayName);
            string str2 = infoThrust;

            infoThrust = str2 + text + ": <color=orange>Variable: uses directly attached segments.</color>\n";

            infoThrust += Localizer.Format("#autoLOC_220759", (ignitionThreshold * 100f).ToString("0.#"));

            return infoThrust;
        }

        void UpdateHighlightColors()
        {
            //highlightActive = true;

            Color c = new Color(1, 1, 1, 1);
            // The following code is because the old way of storing the colors was a number from 0-100, 
            // The new ColorPicker uses a number 0-1

            c.b = HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_2>().highlightBlueA > 1 ? HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_2>().highlightBlueA / 100f : HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_2>().highlightBlueA;
            c.r = HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_2>().highlightRedA > 1 ? HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_2>().highlightRedA / 100f : HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_2>().highlightRedA;
            c.g = HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_2>().highlightGreenA > 1 ? HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_2>().highlightGreenA / 100f : HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_2>().highlightGreenA;

            phl.UpdateHighlightColors(highlightID, c);
        }

        internal void HighlightStack()
        {
            if (/* HighLogic.LoadedSceneIsEditor && */ phl != null)
                foreach (var s in segments)
                    phl.AddPartToHighlight(highlightID, s.part);
        }
        internal void UnHighlightStack()
        {
            if (/* HighLogic.LoadedSceneIsEditor && */ phl != null)
                if (segments != null && highlightID >= 0)
                    foreach (var s in segments)
                        if (s != null)
                            phl.DisablePartHighlighting(highlightID, s.part);
        }

    }
}
