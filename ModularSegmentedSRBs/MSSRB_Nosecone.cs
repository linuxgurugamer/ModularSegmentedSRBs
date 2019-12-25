using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP_Log;

namespace ModularSegmentedSRBs
{
    class MSSRB_Nosecone : PartModule, IPartCostModifier
    {
        KSP_Log.Log Log = new Log("ModularSegmentedSRBs.MSSRB_Nosecone");

        public new MSSRB_Part part
        { get { return base.part as MSSRB_Part; } }


        [KSPAction("Abort!", actionGroup = KSPActionGroup.Abort)]
        public void DoJettison(KSPActionParam param)
        {
            part.decouple();
        }

        [KSPField(isPersistant = true)]
        public bool disableParachute = false;

        [KSPEvent(guiActiveEditor = true, guiActive = false, guiName = "Disable parachute (currently enabled)")]
        public void DisableParachute() { disableParachute = !disableParachute; GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship); }

        [KSPField(isPersistant = true)]
        public bool disableMFC = false;

        [KSPEvent(guiActiveEditor = true, guiActive = false, guiName = "Disable avionics (currently enabled)")]
        public void DisableMFC() { disableMFC = !disableMFC; GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship); }

        [KSPField(isPersistant = true)]
        public bool disablePP = false;

        [KSPEvent(guiActiveEditor = true, guiActive = false, guiName = "Disable sepratrons (currently enabled)")]
        public void DisablePP() { disablePP = !disablePP; GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship); }

        public override void OnStart(StartState stat)
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (!ModSegSRBs.PartAvailable(ModSegSRBs.ParachuteTechName))
                {
                    Events["DisableParachute"].guiActiveEditor = false;
                    disableParachute = true;
                }

                if (!ModSegSRBs.PartAvailable(ModSegSRBs.MFCTechName))
                {
                    Events["DisableMFC"].guiActiveEditor = false;
                    disableMFC = true;
                }

                Log.Info("OnStart, (" + ModSegSRBs.PPTechName + "): " + ModSegSRBs.PartAvailable(ModSegSRBs.PPTechName));
                if (!ModSegSRBs.PartAvailable(ModSegSRBs.PPTechName))
                {
                    Events["DisablePP"].guiActiveEditor = false;
                    disablePP = true;
                }
            }
            part.RestoreVariant();
            base.OnStart(stat);
        }

        void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                switch (disableParachute)
                {
                    case false:
                        Events["DisableParachute"].guiName = "Disable parachute (currently enabled)";
                        break;
                    case true:
                        Events["DisableParachute"].guiName = "Enable parachute (currently disabled)";
                        break;
                }

                switch (disableMFC)
                {
                    case false:
                        Events["DisableMFC"].guiName = "Disable avionics (currently enabled)";
                        break;
                    case true:
                        Events["DisableMFC"].guiName = "Enable avionics (currently disabled)";
                        break;
                }

                switch (disablePP)
                {
                    case false:
                        Events["DisablePP"].guiName = "Disable sepratrons (currently enabled)";
                        break;
                    case true:
                        Events["DisablePP"].guiName = "Enable sepratrons (currently disabled)";
                        break;
                }

            }
        }


        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            float cost = 0;
            float chute = 0;
            float MFT = 0;
            float PP = 0;

            if (part == null)
                return 0;

            if (!disableParachute && ModSegSRBs.PartAvailable(ModSegSRBs.ParachuteTechName))
            {
                chute = 422;
                if (part.segmentWidth > 0.725)
                    chute = 1688;
                if (part.segmentWidth > 1.35)
                    chute = 3798;
                if (part.segmentWidth > 1.975)
                    chute = 6752;
            }

            if (!disableMFC && ModSegSRBs.PartAvailable(ModSegSRBs.MFCTechName))
                MFT = 300;

            if (!disablePP && ModSegSRBs.PartAvailable(ModSegSRBs.PPTechName))
            {
                PP = 9;
                if (part.segmentWidth > 0.725)
                    PP = 75;
                if (part.segmentWidth > 1.35)
                    PP = 600;
                if (part.segmentWidth > 1.975)
                    PP = 4800;
            }

            cost = chute + MFT + PP;
            return cost;
        }

        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

    }
}
