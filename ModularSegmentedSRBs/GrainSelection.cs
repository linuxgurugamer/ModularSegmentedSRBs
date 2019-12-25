#if false
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using UnityEngine;
using ClickThroughFix;
using ToolbarControl_NS;
using KSP_Log;

namespace ModularSegmentedSRBs
{

    public class Grain
    {
        public string name;
        public string descr;
        public Texture2D grain;     // cross section of the grain
        public Texture2D graph;     // Graph showing thrust curve
        public Texture2D joined;    // image with grain and graph combined
        public FloatCurve thrustCurve;

        public Grain(string name, Texture2D grain, Texture2D graph, string descr, FloatCurve thrustCurve = null)
        {
            this.name = name;
            this.grain = grain;
            this.graph = graph;
            this.descr = descr;
            joined = JoinTextures(grain, 20, graph);
            this.thrustCurve = thrustCurve;
        }

        public static Texture2D JoinTextures(Texture2D leftImage, int space, Texture2D rightImage)
        {
            // Create a new writable texture.
            Texture2D result = new Texture2D(leftImage.width + rightImage.width + space, Math.Max(leftImage.height, rightImage.height));

            for (int x = 0; x < leftImage.width; x++)
            {
                for (int y = 0; y < leftImage.height; y++)
                {
                    Color bgColor = leftImage.GetPixel(x, y);
                    result.SetPixel(x, y, bgColor);
                }
            }
            for (int x = 0; x < rightImage.width; x++)
            {
                for (int y = 0; y < rightImage.height; y++)
                {
                    Color bgColor = rightImage.GetPixel(x, y);
                    result.SetPixel(leftImage.width + space + x, y, bgColor);
                }
            }

            result.Apply();
            return result;
        }
    }

    class GrainSelection : MonoBehaviour
    {
        internal MSSRB_Engine parent;

        const int MAIN_WIDTH = 800;
        const int MAIN_HEIGHT = 600;
        const int THUMBNAILSIZE = 200;


        const string GRAINDIR = "GameData/ModularSegmentedSRBs/PluginData/Grains";
        int grainWindowId;

        Rect grainWin;


        List<Grain> ThrustCurves = new List<Grain>();
        Log Log;
        public void Start()
        {
            Log = new Log("GrainSelection");
            grainWindowId = GUIUtility.GetControlID(FocusType.Passive);
            grainWin = new Rect((Screen.width - MAIN_WIDTH) / 2, (Screen.height - MAIN_HEIGHT) / 2, MAIN_WIDTH, MAIN_HEIGHT);

            var grainDirs = Directory.GetDirectories(GRAINDIR).ToList();
            for (int i = 0; i < grainDirs.Count; i++)
            {
                string grainDir = Path.GetFileName(grainDirs[i]);
                Texture2D grain = LoadGrain(grainDir);
                Texture2D graph = LoadGraph(grainDir);
                FloatCurve curve = LoadThrustCurve(grainDir);
                string descr = LoadDescr(grainDir);
                var thrustCurve = new Grain(grainDir, grain, graph, descr, curve);
                ThrustCurves.Add(thrustCurve);
            }
        }

#if false
        internal class CurveEntry
        {
            public float time;
            public float thrust;
            public float tanIn;
            public float tanOut;

            internal CurveEntry(float time, float thrust, float tanIn, float tanOut)
            {
                this.time = time;
                this.thrust = thrust;
                this.tanIn = tanIn;
                this.tanOut = tanOut;
            }
        }
#endif
        internal static FloatCurve LoadThrustCurve(string curveName)
        {
            Log Log = new Log("LoadThrustCurve");
            FloatCurve newCurve = new FloatCurve();
            string path = GRAINDIR + "/" + curveName + "/" + "thrustCurve.txt";
            if (!File.Exists(path))
            {
                Log.Error("Missing ThrustCurve file: " + path);
                return null;
            }
            var lines = File.ReadAllLines(path);
           
            foreach (var line in lines)
            { 

                var values = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    float time = float.Parse(values[2]);
                    float thrust = float.Parse(values[3]);
                    float tanIn = 0, tanOut = 0;
                    if (values.Length >=5)
                        tanIn = float.Parse(values[4]);
                    if (values.Length >= 6)
                        tanOut = float.Parse(values[5]);
                   
                    newCurve.Add(time, thrust, tanIn, tanOut);
                    Log.Info(curveName + " thrustcurve: " + time + " " + thrust + " " + tanIn + " " + tanOut);
                }
                catch (Exception e)
                {
                    Log.Error("[MSSRB.LoadThrustCurve] Exception parsing float curve[" + curveName + "]: " + e.Message);
                }

            }

            return newCurve;
        }

        Texture2D MakeThumbnailFrom(string origImageFile, int thumbnailsize)
        {
            Log.Info("Loading: " + origImageFile);
            if (!File.Exists(origImageFile + ".png"))
                return null;

            byte[] fileData = System.IO.File.ReadAllBytes(origImageFile + ".png");
            Texture2D image = new Texture2D(2, 2);
            image.LoadImage(fileData); //..this will auto-resize the texture dimensions.


            float h = (float)image.height / (float)image.width * thumbnailsize;

            //if (HighLogic.CurrentGame.Parameters.CustomParams<KL_11>().useBilinear)
            TextureScale.Bilinear(image, (int)thumbnailsize, (int)h);
            //else
            //    TextureScale.Point(screenshot, (int)thumbnailsize, (int)h);

            return image;
        }

        string Wrap(string text, int margin)
        {
            int start = 0, end;
            var lines = new List<string>();
            text = Regex.Replace(text, @"\s", " ").Trim();

            while ((end = start + margin) < text.Length)
            {
                while (text[end] != ' ' && end > start)
                    end -= 1;

                if (end == start)
                    end = start + margin;

                lines.Add(text.Substring(start, end - start));
                start = end + 1;
            }

            if (start < text.Length)
                lines.Add(text.Substring(start));
            string str = "";
            foreach (var l in lines)
            {
                if (str != "")
                    str += "\n";
                str += l;
            }
            return str;
        }

        string LoadDescr(string curveName)
        {
            string path = GRAINDIR + "/" + curveName + "/" + curveName + ".txt";
            if (File.Exists(path))
            {
                return Wrap(File.ReadAllText(path), 80);
            }
            return "";
        }

        Texture2D LoadGrain(string curveName)
        {
            string path = GRAINDIR + "/" + curveName + "/" + curveName;
            Texture2D img = MakeThumbnailFrom(path, THUMBNAILSIZE);
            return img;
        }
        Texture2D LoadGraph(string curveName)
        {
            string path = GRAINDIR + "/" + curveName + "/" + curveName + "Graph";
            Texture2D img = MakeThumbnailFrom(path, THUMBNAILSIZE);
            return img;
        }

        public void OnGUI()
        {
            if (tooltip != "")
            {
                SetupTooltip();
                ClickThruBlocker.GUIWindow(1234, tooltipRect, TooltipWindow, "");
            }
            grainWin = ClickThruBlocker.GUILayoutWindow(grainWindowId, grainWin, Win, "Grain Selection");
        }

        void SelectGrain(Grain grain)
        {
            parent.SetGrain = grain;
            CloseWindow();
        }

        void CloseWindow()
        {
            Destroy(this);
        }

        Vector2 pos;
        public void Win(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            pos = GUILayout.BeginScrollView(pos);

            foreach (var thrustCurve in ThrustCurves)
            {
                GUILayout.BeginHorizontal();
                var content = new GUIContent(thrustCurve.name, thrustCurve.descr);
                if (GUILayout.Button(content, GUILayout.Width(100), GUILayout.Height(thrustCurve.joined.height)))
                {
                    SelectGrain(thrustCurve);
                }
                if (GUILayout.Button(thrustCurve.joined))
                {
                    SelectGrain(thrustCurve);
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel"))
            {
                CloseWindow();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (Event.current.type == EventType.Repaint && GUI.tooltip != tooltip)
                tooltip = GUI.tooltip;
            GUI.DragWindow();
        }

        string tooltip = "";
        bool drawTooltip = true;
        // Vector2 mousePosition;
        Vector2 tooltipSize;
        float tooltipX, tooltipY;
        Rect tooltipRect;
        void SetupTooltip()
        {
            Vector2 mousePosition;
            mousePosition.x = Input.mousePosition.x;
            mousePosition.y = Screen.height - Input.mousePosition.y;
            //  Log.Info("SetupTooltip, tooltip: " + tooltip);
            if (tooltip != null && tooltip.Trim().Length > 0)
            {
                tooltipSize = HighLogic.Skin.label.CalcSize(new GUIContent(tooltip));
                tooltipX = (mousePosition.x + tooltipSize.x > Screen.width) ? (Screen.width - tooltipSize.x) : mousePosition.x;
                tooltipY = mousePosition.y;
                if (tooltipX < 0) tooltipX = 0;
                if (tooltipY < 0) tooltipY = 0;
                tooltipRect = new Rect(tooltipX - 1, tooltipY - tooltipSize.y, tooltipSize.x + 4, tooltipSize.y);
            }
        }

        void TooltipWindow(int id)
        {
            GUI.Label(new Rect(2, 0, tooltipRect.width - 2, tooltipRect.height), tooltip, HighLogic.Skin.label);
        }

    }
}
#endif