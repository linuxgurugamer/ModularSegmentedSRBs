using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP.Localization;
using UnityEngine;
using KSP_Log;
using KSP.UI.Screens;
using KSP_PartHighlighter;

namespace ModularSegmentedSRBs
{
    internal class Segment
    {
        internal MSSRB_Part part;
        internal double ratio;
        internal float segmentHeight;

        public Segment(MSSRB_Part p, float segHeight)
        {
            part = p;
            ratio = 0;

            segmentHeight = segHeight;
        }
    }

    public class MSSRB_Engine : ModuleEnginesFX, IPartCostModifier
    {
        public new MSSRB_Part part { get { return base.part as MSSRB_Part; } }

        Log Log = new Log("ModularSegmentedSRBs.MSSRB_Engine");

        public float baseHeatProduction = 545;

        List<Segment> segments = new List<Segment>();

        float invTotalSegmentHeight = 0;
        double totalMaxThrust = 0;
        double totalFuelMass = 0;
        double totalFuelFlow = 0f;
        float failureChance = 0;

        [KSPField]
        public float thrustModifier = 1;

        [KSPField(guiFormat = "F5", guiActive = true, guiName = "Maximum Thrust", guiActiveEditor = true)]
        float maxDisplayThrust = 0f;

        [KSPField(guiFormat = "F5", guiActive = true, guiName = "Total Segment Height", guiActiveEditor = true)]
        float totalSegmentHeight = 0f;

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
#if false
#region // Grain

        [KSPField(isPersistant = true)]
        string grainName = "Default";
 
        Grain grain;

        public Grain SetGrain
        {
            set {
                grain = value;
                grainName = value.name;
                SetThrustCurve(null, grain.thrustCurve);
                Events["SelectFuelGrain"].guiName = "Sel. Fuel Grain/Thrust Curve (" + grainName + ")";
            }
        }

        [KSPEvent(guiActiveEditor = true, guiActive = false, guiName = "Sel. Fuel Grain/Thrust Curve")]
        public void SelectFuelGrain()
        {
            var grainSelObj = gameObject.AddComponent<GrainSelection>();
            grainSelObj.parent = this;
        }
#endregion
#endif

        int updateSegmentsIn = -1;

        public void ScheduleSegmentUpdate(string s, int i = 5)
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
        int propIndx = 0;

        // Cost is scaled approximately by the cube of the change in size
        // The values below are along the lines that Tweakscale uses
        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            float cost = defaultCost;
            /* The following calculations are precalculated, just here for documentation
            if (part.segmentWidth > 0.725f)
                cost = cost * 7.733f;
            if (part.segmentWidth > 1.35f)
                cost = cost * 3.375f;
            if (part.segmentWidth > 1.975f)
                cost = cost * 2.367f;
                */
            cost = 0;
            Log.Info("GetModuleCost, part.segmentWidth: " + part.segmentWidth + ", defaultCost: " + defaultCost + ", finalCost: " + cost);

            return cost;
        }


        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

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

#if THRUSTVARIABILITY
        static float ChangeThrust(float changeMult, float maxThrust, float thrustVariability = 0)
        {
            float thrust = maxThrust * changeMult;
            thrust += thrust * thrustVariability;
            return thrust;
        }
#else
        static float ChangeThrust(float changeMult, float maxThrust)
        {
            return maxThrust * changeMult;
        }
#endif

#if THRUSTVARIABILITY
        internal void ChangeUsage(float changeMult, float thrustVariability = 0)
#else
        internal void ChangeUsage(float changeMult) //, float thrustVariability = 0)
#endif
        {
            atmosphereCurve = origAtmosphereCurve;
            maxThrust = origMaxThrust;
#if THRUSTVARIABILITY
            ChangeUsage(changeMult, ref maxThrust, ref atmosphereCurve, thrustVariability);
#else
            ChangeUsage(changeMult, ref maxThrust, ref atmosphereCurve);
#endif
        }
#if THRUSTVARIABILITY
        internal static void ChangeUsage(float changeMult, ref float maxThrust, ref FloatCurve atmosphereCurve, float thrustVariability = 0)
#else
        internal static void ChangeUsage(float changeMult, ref float maxThrust, ref FloatCurve atmosphereCurve) //, float thrustVariability = 0)
#endif
        {
#if THRUSTVARIABILITY
            maxThrust = ChangeThrust(changeMult, maxThrust, thrustVariability); // Needs to have the thrustVariability added here
#else
            maxThrust = ChangeThrust(changeMult, maxThrust);
#endif
            atmosphereCurve = ChangeFloatCurve(changeMult, atmosphereCurve);
        }

#if false
        void SetThrustCurve(string grainName = "", FloatCurve thrustCurve = null)
        {
            if (grainName != null && grainName != "" && grainName != "Default")
            {
                thrustCurve = GrainSelection.LoadThrustCurve(grainName);
            }
            if (thrustCurve != null)
            {
                Log.Info("Setting Thrust curve");
                this.thrustCurve = thrustCurve;
                useThrustCurve = true;
            }
        }
#endif

        internal void SetEngineHealth(float changeMult)
        {
            engineHealth = changeMult;
        }

        public override void OnStart(StartState state)
        {
            Log.Info("OnStart");
            base.OnStart(state);
            //Start();
        }

        public void Start()
        {
            Log.Info("Start");
            part.RestoreVariant();
#if false
            SetThrustCurve(grainName, null);
#endif

            Events["TriggerEngineFailureEvent"].active = HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().devMode;
            Actions["TriggerEngineFailureAction"].active = HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().devMode;

#if false
            foreach (var e in Events)
                Log.Info("Event: " + e.name + ", guiName = " + e.guiName);
            foreach (var a in Actions)
                Log.Info("Action: " + a.name + ", guiName: " + a.guiName);
            foreach (var f in Fields)
                Log.Info("Field: " + f.name + ", guiName: " + f.guiName);
#endif

            Fields["independentThrottle"].guiActive =
                Fields["independentThrottle"].guiActiveEditor = false;
            Fields["thrustPercentage"].guiActive =
                Fields["thrustPercentage"].guiActiveEditor = false;



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

            PartModuleList pml = part.Modules;
            if (pml.Contains("MSSRB_Fuel_Segment"))
            {
                MSSRB_Fuel_Segment moduleFuelSegment = pml["MSSRB_Fuel_Segment"] as MSSRB_Fuel_Segment;
                part.segmentWidth = moduleFuelSegment.part.segmentWidth;
            }
            else
                Log.Info("MSSRB_Engine, missing MSSRB_Fuel_Segment");

            foreach (var p in propellants)
            {
                if (p.name == ModSegSRBs.BurnablePropellant)
                {
                    break;
                }
                burnablePropIndx++;
            }
            foreach (var p in propellants)
            {
                if (p.name == ModSegSRBs.Propellant)
                {
                    break;
                }
                propIndx++;
            }
            ModSegSRBs.GetExtraInfo(part.baseVariant, ref part.segmentHeight, ref part.segmentWidth);

            switch (HighLogic.LoadedScene)
            {
                case GameScenes.EDITOR:
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
                            /* PartModuleList */
                            pml = s.part.Modules;
                            MSSRB_Fuel_Segment moduleFuelSegment = pml["MSSRB_Fuel_Segment"] as MSSRB_Fuel_Segment;

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
            ModSegSRBs.GetExtraInfo(variant, ref this.part.segmentHeight, ref this.part.segmentWidth);

            ScheduleSegmentUpdate("onEditorVariantApplied");
#if true
            MonoUtilities.RefreshContextWindows(part);
#else
            MonoUtilities.RefreshPartContextWindow(part);
#endif
        }

        void onEditorShipModified(ShipConstruct construct) { ScheduleSegmentUpdate("onEditorShipModified"); }

        bool CheckPartResources(MSSRB_Part part)
        {
            // Check the nextPart's resources and Modules
            // Check for the correct propellant and that it has the MSSRB_Fuel_Segment module
            // If so, add it to the segments list and accumulate some values for the motor
            // then update the propellant value amount in the fuel segment
            PartResourceList prl = part.Resources;
            PartModuleList pml = part.Modules;
            if (prl.Contains(ModSegSRBs.Propellant) && pml.Contains<MSSRB_Fuel_Segment>())
            {
                Log.Info("CheckPartResources, Part: " + part.partInfo.title + ", " + ModSegSRBs.Propellant + " found, " + " MSSRB_Fuel_Segment found");
                MSSRB_Fuel_Segment moduleFuelSegment = pml["MSSRB_Fuel_Segment"] as MSSRB_Fuel_Segment;
                segments.Add(new Segment(part, moduleFuelSegment.part.segmentHeight));
                totalFuelMass += moduleFuelSegment.MaxSolidFuel();  // prl[ModSegSRBs.Propellant].maxAmount;
                totalMaxThrust += moduleFuelSegment.GetMaxThrust();
                Log.Info("CheckPartResources, moduleFuelSegment.part.segmentHeight: " + moduleFuelSegment.part.segmentHeight);
                totalSegmentHeight += moduleFuelSegment.part.segmentHeight;

                Log.Info("CheckPartResources, totalFuelMass: " + totalFuelMass + ", totalMaxThrust: " + totalMaxThrust + ", totalSegmentHeight: " + totalSegmentHeight);

                if (totalSegmentHeight > 0)
                    invTotalSegmentHeight = 1 / totalSegmentHeight;
                else
                    invTotalSegmentHeight = 0;

                moduleFuelSegment.baseEngine = this;

                if (CoM == null && HighLogic.LoadedSceneIsEditor)
                {
                    prl[ModSegSRBs.Propellant].amount =
                        prl[ModSegSRBs.Propellant].maxAmount = 0;

                    prl[ModSegSRBs.BurnablePropellant].amount =
                        prl[ModSegSRBs.BurnablePropellant].maxAmount = moduleFuelSegment.MaxSolidFuel();
                    Log.Info("(2) Propellant Setting part.amount = 0, BurnablePropellant maxAmount set to: " + prl[ModSegSRBs.BurnablePropellant].maxAmount);
                }
                else
                {
                    prl[ModSegSRBs.Propellant].amount =
                        prl[ModSegSRBs.Propellant].maxAmount = moduleFuelSegment.MaxSolidFuel();

                    prl[ModSegSRBs.BurnablePropellant].amount =
                        prl[ModSegSRBs.BurnablePropellant].maxAmount = 0;
                }

                Log.Info("totalMaxThrust: " + totalMaxThrust + ", totalFuelMass: " + totalFuelMass);

                // Set the baseEngine in the segment ends here, do it in a loop
                // because there can be either one or two ends
                foreach (var p in pml)
                {
                    if (p is MSSRB_SegmentEnds)
                    {
                        MSSRB_SegmentEnds mssrb = p as MSSRB_SegmentEnds;
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
            totalFuelMass = 0;
            segments.Clear();
            Part curPart = this.part;
            Part nextPart;

            AttachNode topAttachNode = this.part.FindAttachNode("top");

            CheckPartResources(this.part as MSSRB_Part);

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
                if (!CheckPartResources(nextPart as MSSRB_Part))
                {
                    Log.Info("CheckPartResources returns false");
                    break;
                }
                curPart = nextPart;
                topAttachNode = nextPart.FindAttachNode("top");
                if (topAttachNode == null)
                    break;
            }

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
            maxDisplayThrust = maxThrust;
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

            if (part.segmentWidth > 0)
            {
                float lengthToWidthRatio = totalSegmentHeight / part.segmentWidth;
                heatProduction = baseHeatProduction + 5 * lengthToWidthRatio;

                if (lengthToWidthRatio > HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().maxSafeWidthToLengthRatio)
                {
                    float excess = totalSegmentHeight - part.segmentWidth * HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().maxSafeWidthToLengthRatio;
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

#if true
            MonoUtilities.RefreshContextWindows(part);
#else
            MonoUtilities.RefreshPartContextWindow(part);
#endif
            GameEvents.onChangeEngineDVIncludeState.Fire(this);
        }

        internal void SetBrokenSRB(float thrust, float percentage, double abortFuelAmt)
        {
            Log.Info("SetBrokenSRB, abortFuelAmt: " + abortFuelAmt);
            maxThrust = thrust;
            ChangeUsage(percentage, ref maxThrust, ref atmosphereCurve);
            brokenSRB = true;

            // part.Resources[ModSegSRBs.AbortedPropellant].maxAmount =
            //                       part.Resources[ModSegSRBs.AbortedPropellant].amount = abortFuelAmt;

            // part.Resources[ModSegSRBs.Propellant].amount = 
            //     part.Resources[ModSegSRBs.BurnablePropellant].amount = abortFuelAmt;

            //part.Resources[ModSegSRBs.BurnablePropellant].maxAmount =
            //                      part.Resources[ModSegSRBs.BurnablePropellant].maxAmount = 0;

            List<MSSRB_SegmentEnds> smfLst = part.Modules.GetModules<MSSRB_SegmentEnds>();
            //foreach (var se in smfLst)
            {
                // if (se.attachNode == null || se.attachNode == "")
                {
                    //  Log.Info("SetBrokenSRB, activating");
                    //  se.ActivateEngine2(maxThrust * percentage, maxFuelFlow);
                }

            }
        }

        void UpdateSegmentsInFlight()
        {
            if (brokenSRB)
            {
                Log.Info("UpdateSegmentsInFlight, part.Resources[ModSegSRBs.BurnablePropellant].amount: " + part.Resources[ModSegSRBs.BurnablePropellant].amount);
                Log.Info("UpdateSegmentsInFlight, part.Resources[ModSegSRBs.AbortedPropellant].amount: " + part.Resources[ModSegSRBs.AbortedPropellant].amount);

            }
            if (this.EngineIgnited && !brokenSRB)
            {
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
#if THRUSTVARIABILITY
                if (engineHealth < oldEngineHealth|| HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().hasThrustVariability)
#else
                if (engineHealth < oldEngineHealth)
#endif
                {
                    oldEngineHealth = engineHealth;
#if THRUSTVARIABILITY
                    if (HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().hasThrustVariability)
                    {
                        fEngRunTime += TimeWarp.fixedDeltaTime;
                        var v = HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().maxVariability * Mathf.Sin(fEngRunTime * Mathf.PI * 2 / ModSegSRBs.pressurePulseHertz);
                        Log.Info("ThrustVariability: " + v);
                        ChangeUsage(engineHealth, v);                        
                    } else
#endif
                    {
                        ChangeUsage(engineHealth);
                    }
                }

                PartResource secondPartResource = null;

                totalFuelFlow = 0;

                double totalCurrentFuel = 0;
                double fuelFlow = 0;
                double totalMaxFuel = 0;
                var maxFuelFlow = getMaxFuelFlow(propellants[burnablePropIndx]) * Time.fixedDeltaTime - part.Resources[ModSegSRBs.BurnablePropellant].amount + 0.05f;


                for (int i = 0; i < segments.Count; i++)
                {
                    Segment segment = segments[i];
                    MSSRB_Fuel_Segment moduleFuelSegment = segment.part.Modules["MSSRB_Fuel_Segment"] as MSSRB_Fuel_Segment;
                    Log.Info("UpdateSegmentsInFlight, segmentheight: " + segment.segmentHeight);
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

                        if (((MSSRB_SegmentEnds)segment.part.Modules["MSSRB_SegmentEnds"]).EngineIgnited)
                        {
                            secondPartResource = segment.part.Resources[ModSegSRBs.BurnablePropellant];
                        }
                        else
                            secondPartResource = null;
                    }
                }

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

        int enableEngineerReport = -11;
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
                    //UpdateSegments();
                    //updateSegmentsIn = -1;
                    updateSegmentsIn = 5;
                }
            }
           if (leftClick)
            {
                if (enableEngineerReport-- == 0 )
                {
                    EngineersReport.Instance.appLauncherButton.Enable();
                }
                if (enableEngineerReport == -3)
                {
                    EngineersReport.Instance.appLauncherButton.onHover();
                }
                if (enableEngineerReport == -6)
                {
                    EngineersReport.Instance.appLauncherButton.onLeftClick();
                    // Following needed because it will be toggled by the "onLeftClick()" method below
                    leftClick = !leftClick;
                }
            }
            if (updateSegmentsIn-- == 0)
            {
                UpdateSegments();
                if (leftClick)
                {
                    GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartTweaked, part);

                    EngineersReport.Instance.appLauncherButton.Disable();
                    enableEngineerReport = 3;
                }
            }
            
        }

        bool engineersReportInitialized = false;
        bool leftClick = false;

        void onLeftClick()
        {
            Log.Info("onLeftClick, leftClick:" + leftClick);
            leftClick = !leftClick;
        }

        new void FixedUpdate()
        {
            switch (HighLogic.LoadedScene)
            {
                case GameScenes.FLIGHT:
                    Log.Info("FixedUpdate, useThrustCurve: " + useThrustCurve);
                    UpdateSegmentsInFlight();
                    break;
                case GameScenes.EDITOR:
                    if (!engineersReportInitialized && EngineersReport.Ready)
                    {
                        EngineersReport.Instance.appLauncherButton.onLeftClick += onLeftClick;
                        engineersReportInitialized = true;                    
                    }
                    
                    UpdateSegmentsInEditor();
                    break;
            }

            base.FixedUpdate();
        }

        public new string GetModuleTitle()
        {
            return "MSSRB_Engine";
        }

        public override string GetModuleDisplayName()
        {
            return "MSSRB_Engine";
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

            Propellant propellant = propellants[propIndx];
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
