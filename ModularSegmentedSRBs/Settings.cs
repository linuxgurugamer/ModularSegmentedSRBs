
using System.Collections;
using System.Reflection;
using UnityEngine;
using KSPColorPicker;

namespace ModularSegmentedSRBs
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    // HighLogic.CurrentGame.Parameters.CustomParams<MSSRB_1>().hasThrustVariability
    public class MSSRB_1 : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "General"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Modular Segmented SRBs"; } }
        public override string DisplaySection { get { return "Modular Segmented SRBs"; } }
        public override int SectionOrder { get { return 3; } }
        public override bool HasPresets { get { return true; } }


#if THRUSTVARIABILITY
        [GameParameters.CustomParameterUI("Thrust Variability",
            toolTip = "When enabled, there is a small but noticable pulsing to the thrust, increasing as time goes on")]
        public bool hasThrustVariability = true;

        [GameParameters.CustomFloatParameterUI("Variability multiplier", displayFormat = "F3", minValue = 0.025f, maxValue = 2f,
            toolTip = "This is the max range of the variability of the thrust, in percent")]
        public float maxVariability = 2f;
#endif

        [GameParameters.CustomFloatParameterUI("Max safe Height:Width ratio", displayFormat = "F2", minValue = 6f, maxValue = 12f,
            toolTip = "When the length of the SRB segments divided by the diameter exceeds this number,\na failure of the rocket is possible.  The higher it is exceeded, the\ngreater the chance for a failure")]
        public float maxSafeWidthToLengthRatio = 8f;

        [GameParameters.CustomParameterUI("Warn on excessive length",
            toolTip = "Provide a warning when the height of the SRBs is longer than safe max height")]
        public bool warnOnExcessiveHeight = true;


        [GameParameters.CustomParameterUI("BetterSRBs compatibility",
            toolTip = "Set maximum thrust using same formula as BetterSRBs uses")]
        public bool BetterSRBsCompatibility = true;


        public float explosionChance = .25f;
        [GameParameters.CustomFloatParameterUI("Chance of explosion during failure (%)", displayFormat = "N0", minValue = 0, maxValue = 100, stepCount = 1, asPercentage = false,
            toolTip = "Higher number = greater chance of explosion")]
        public float ExplosionChance
        {
            get { return explosionChance * 100; }
            set { explosionChance = value / 100.0f; }
        }



        public float failureChance = 0f;
        [GameParameters.CustomFloatParameterUI("Base failure chance per minute (%)", displayFormat = "N0", minValue = 0, maxValue = 100, stepCount = 1, asPercentage = false,
            toolTip = "Higher number = greater chance of failure.  This is the chance of a failure during 1 minute of burn time. Chance of failure will never be less than this value")]
        public float FailureChance
        {
            get { return failureChance * 100; }
            set { failureChance = value / 100.0f; }
        }


        [GameParameters.CustomParameterUI("Enable alarm sound",
            toolTip = "Sounds a loud alarm when a failure occurs")]
        public bool alarm = true;

        float masterVol = 0.5f;
        [GameParameters.CustomFloatParameterUI("Master Volume (%)", displayFormat = "N0", minValue = 0, maxValue = 100, stepCount = 1, asPercentage = false)]
        public float masterVolume
        {
            get { return masterVol * 100; }
            set { masterVol = value / 100.0f; }
        }

        [GameParameters.CustomParameterUI("Dev/test mode",
            toolTip = "Enables failure actions which can be used in Action Groups and PAW menu, useful for flight testing")]
        public bool devMode = false;
       
        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            switch (preset)
            {
                case GameParameters.Preset.Easy:
#if THRUSTVARIABILITY
                    hasThrustVariability = false;
#endif
                    maxSafeWidthToLengthRatio = 12f;
                    warnOnExcessiveHeight = true;
                    break;

                case GameParameters.Preset.Normal:
#if THRUSTVARIABILITY
                    hasThrustVariability = true;
                    maxVariability = 0.05f;
#endif
                    maxSafeWidthToLengthRatio = 10f;
                    warnOnExcessiveHeight = true;
                    break;

                case GameParameters.Preset.Moderate:
#if THRUSTVARIABILITY
                    hasThrustVariability = true;
                    maxVariability = 0.1f;
#endif
                    maxSafeWidthToLengthRatio = 8f;
                    warnOnExcessiveHeight = true;
                    break;

                case GameParameters.Preset.Hard:
#if THRUSTVARIABILITY
                    hasThrustVariability = true;
                    maxVariability = 0.15f;
#endif
                    maxSafeWidthToLengthRatio = 7f;
                    warnOnExcessiveHeight = false;
                    break;

            }
        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {

            return true;
        }


        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {

            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }


    public class MSSRB_2 : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Color"; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Modular Segmented SRBs"; } }
        public override string DisplaySection { get { return "Modular Segmented SRBs"; } }
        public override int SectionOrder { get { return 3; } }
        public override bool HasPresets { get { return false; } }



        [GameParameters.CustomParameterUI("Show Color Picker for A",
           toolTip = "Show the Color Picker dialog for the first color in the cycle")]
        public bool showColorPickerA = false;


        [GameParameters.CustomFloatParameterUI("Red value", minValue = 0, maxValue = 100f, stepCount = 101, displayFormat = "F4",
            toolTip = "Amount of red to be used in the highlight. range is from 0-100")]
        public float highlightRedA = 1f;

        [GameParameters.CustomFloatParameterUI("Green value", minValue = 0, maxValue = 100f, stepCount = 101, displayFormat = "F4",
            toolTip = "Amount of green to be used in the highlight. range is from 0-100")]
        public float highlightGreenA = 0f;

        [GameParameters.CustomFloatParameterUI("Blue value", minValue = 0, maxValue = 100f, stepCount = 101, displayFormat = "F4",
            toolTip = "Amount of blue to be used in the highlight. range is from 0-100")]
        public float highlightBlueA = 0f;

        //====================================
#if false
        [GameParameters.CustomParameterUI("Show Color Picker for B",
           toolTip = "Show the Color Picker dialog for the second color in the cycle")]
        public bool showColorPickerB = false;


        [GameParameters.CustomFloatParameterUI("Red value", minValue = 0, maxValue = 100f, stepCount = 101, displayFormat = "F4",
            toolTip = "Amount of red to be used in the highlight. range is from 0-100")]
        public float highlightRedB = 1f;

        [GameParameters.CustomFloatParameterUI("Green value", minValue = 0, maxValue = 100f, stepCount = 101, displayFormat = "F4",
            toolTip = "Amount of green to be used in the highlight. range is from 0-100")]
        public float highlightGreenB = 0f;

        [GameParameters.CustomFloatParameterUI("Blue value", minValue = 0, maxValue = 100f, stepCount = 101, displayFormat = "F4",
            toolTip = "Amount of blue to be used in the highlight. range is from 0-100")]
        public float highlightBlueB = 0f;

        //====================================


        [GameParameters.CustomParameterUI("Show Color Picker for C",
           toolTip = "Show the Color Picker dialog for the third color in the cycle")]
        public bool showColorPickerC = false;


        [GameParameters.CustomFloatParameterUI("Red value", minValue = 0, maxValue = 100f, stepCount = 101, displayFormat = "F4",
            toolTip = "Amount of red to be used in the highlight. range is from 0-100")]
        public float highlightRedC = 1f;

        [GameParameters.CustomFloatParameterUI("Green value", minValue = 0, maxValue = 100f, stepCount = 101, displayFormat = "F4",
            toolTip = "Amount of green to be used in the highlight. range is from 0-100")]
        public float highlightGreenC = 0f;

        [GameParameters.CustomFloatParameterUI("Blue value", minValue = 0, maxValue = 100f, stepCount = 101, displayFormat = "F4",
            toolTip = "Amount of blue to be used in the highlight. range is from 0-100")]
        public float highlightBlueC = 0f;
#endif


        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {

        }

        string activeColorPicker = "";
        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (member.Name.StartsWith("highlight")) return false;

            if (showColorPickerA)
            {
                activeColorPicker = "A";
                showColorPickerA = false;
                Color c = new Color(1, 1, 1, 1);
                c.b = highlightBlueA;
                c.g = highlightGreenA;
                c.r = highlightRedA;
                KSP_ColorPicker.CreateColorPicker(c, false, "ColorCircle");
            }
#if false
            if (showColorPickerB)
            {
                activeColorPicker = "B";
                showColorPickerB = false;
                Color c = new Color(1, 1, 1, 1);
                c.b = highlightBlueB;
                c.g = highlightGreenB;
                c.r = highlightRedB;
                KSP_ColorPicker.CreateColorPicker(c, false, "ColorCircle");
            }
            if (showColorPickerC)
            {
                activeColorPicker = "C";
                showColorPickerC = false;
                Color c = new Color(1, 1, 1, 1);
                c.b = highlightBlueC;
                c.g = highlightGreenC;
                c.r = highlightRedC;
                KSP_ColorPicker.CreateColorPicker(c, false, "ColorCircle");
            }
#endif
            return true;
        }

        bool unread = false;
        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (KSP_ColorPicker.showPicker)
            {
                unread = true;
                KSP_ColorPicker.colorPickerInstance.PingTime();
                return false;
            }
            else
            {
                if (KSP_ColorPicker.success && unread)
                {
                    unread = false;
                    switch (activeColorPicker)
                    {
                        case "A":
                            highlightBlueA = KSP_ColorPicker.SelectedColor.b;
                            highlightGreenA = KSP_ColorPicker.SelectedColor.g;
                            highlightRedA = KSP_ColorPicker.SelectedColor.r;
                            break;
#if false
                        case "B":
                            highlightBlueB = KSP_ColorPicker.SelectedColor.b;
                            highlightGreenB = KSP_ColorPicker.SelectedColor.g;
                            highlightRedB = KSP_ColorPicker.SelectedColor.r;
                            break;
                        case "C":
                            highlightBlueC = KSP_ColorPicker.SelectedColor.b;
                            highlightGreenC = KSP_ColorPicker.SelectedColor.g;
                            highlightRedC = KSP_ColorPicker.SelectedColor.r;
                            break;
#endif
                    }
                }
            }
            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }
}
