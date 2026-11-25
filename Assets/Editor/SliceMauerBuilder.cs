using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ConcreteLife.EditorTools
{
    /// <summary>
    /// Baut die Szene "Slice_Mauer" (heutige, kahle Hafenmauer) reproduzierbar auf.
    /// Menue: Concrete Life > Slice Mauer.
    /// Grundsatz: nichts darf sich sichtbar wiederholen -> jede Platte hat ihre eigene Textur,
    /// Caustics und Boden sind grossflaechig und nicht gekachelt.
    /// Alles hier sind Platzhalter, die spaeter durch Katjas Blender-Modelle ersetzt werden koennen.
    /// </summary>
    public static class SliceMauerBuilder
    {
        const string TexDir = "Assets/Textures/Slice_Mauer";
        const string PanelTexDir = TexDir + "/Platten";
        const string CausticDir = TexDir + "/Caustics";
        const string MatDir = "Assets/Materials/Slice_Mauer";
        const string ScenePath = "Assets/Scenes/Slice_Mauer.unity";
        const string VolumePath = "Assets/Settings/Slice_Mauer_Volume.asset";

        // Mauer: hochkant stehende Fertigteile ueber die volle Hoehe + Kopfbalken
        // Mauer deutlich breiter als der Kamerabereich, damit ihre Enden nie ins Bild kommen
        const float WallLeft = -30f, WallRight = 30f;
        const float PanelH = 7.2f;
        const float BeamH = 1.0f, BeamDepth = 1.0f;
        public const float WaterLevel = 7.6f;

        // Boden: eine durchgehende Textur fuer die sichtbare Flaeche
        const float GroundW = 64f, GroundD = 16f;

        // Caustics: grosses, nicht sichtbar gekacheltes Muster
        const int CausticFrames = 24;
        const int CausticRes = 1024;
        const int CausticCells = 28;
        const float CausticWorldSize = 20f;

        static readonly Color FogColor = new Color(0.25f, 0.38f, 0.32f);

        // Kamera leicht von unten, Blick an der Mauer hoch Richtung Licht
        // Startstellung (Taucherin auf mittlerer Hoehe); waehrend des Spiels steuert CameraFollow Hoehe und Neigung
        static readonly Vector3 CamPos = new Vector3(0f, 1.6f, -11f);
        const float CamPitch = -11f, CamFov = 60f;
        // Richtung zur sichtbaren Sonne (vor der Kamera, oben). Die Strahlen laufen von dort herab.
        static readonly Vector3 SunViewDir = new Vector3(0.1f, 0.7f, 0.7f).normalized;

        // ------------------------------------------------------------------ Layout der Platten

        struct Anchor { public Vector2 uv; public float length; public float strength; }

        class PanelInfo
        {
            public int index;
            public float x, width, zOffset, rotY, rotZ;
            public int seed;
            public List<Anchor> anchors = new List<Anchor>();
        }

        /// Feste, aber unregelmaessige Aufteilung: Breiten, Fugen, Versatz und Anker sind je Platte verschieden.
        static List<PanelInfo> Layout()
        {
            var rnd = new System.Random(11);
            var list = new List<PanelInfo>();
            float x = WallLeft;
            int i = 0;
            while (x < WallRight)
            {
                float w = 2.3f + (float)rnd.NextDouble() * 0.4f;
                float joint = 0.02f + (float)rnd.NextDouble() * 0.06f;
                var p = new PanelInfo
                {
                    index = i,
                    width = w,
                    x = x + w * 0.5f,
                    zOffset = (float)rnd.NextDouble() * 0.05f,
                    rotY = ((float)rnd.NextDouble() - 0.5f) * 0.6f,
                    rotZ = ((float)rnd.NextDouble() - 0.5f) * 0.3f,
                    seed = 1000 + i * 137,
                };
                // Hebepunkte oben (1-2) + zufaellige Anker (0-2), Rostspuren verschieden lang
                int lift = 1 + rnd.Next(2);
                for (int a = 0; a < lift; a++)
                    p.anchors.Add(new Anchor
                    {
                        uv = new Vector2(0.15f + (float)rnd.NextDouble() * 0.7f, 0.86f + (float)rnd.NextDouble() * 0.09f),
                        length = 0.4f + (float)rnd.NextDouble() * 2.2f,
                        strength = 0.5f + (float)rnd.NextDouble() * 0.5f,
                    });
                int extra = rnd.Next(3);
                for (int a = 0; a < extra; a++)
                    p.anchors.Add(new Anchor
                    {
                        uv = new Vector2(0.1f + (float)rnd.NextDouble() * 0.8f, 0.15f + (float)rnd.NextDouble() * 0.6f),
                        length = 0.2f + (float)rnd.NextDouble() * 1.2f,
                        strength = 0.3f + (float)rnd.NextDouble() * 0.6f,
                    });
                list.Add(p);
                x += w + joint;
                i++;
            }
            return list;
        }

        enum TexKind { Color, Linear, Normal }
        struct PendingTex { public string path; public TexKind kind; public TextureWrapMode wrap; public int size; }
        static readonly List<PendingTex> s_pending = new List<PendingTex>();

        // ------------------------------------------------------------------ Texturen

        [MenuItem("Concrete Life/Slice Mauer/1 - Texturen erzeugen")]
        public static void GenerateTextures()
        {
            int n = Layout().Count;
            GeneratePanelTextures(0, n);
            GenerateEnvironmentTextures();
            GenerateCausticTextures(0, CausticFrames);
        }

        /// Platten-Texturen [from, to) – einzeln aufrufbar, weil jede Platte einmalig ist und etwas dauert.
        public static void GeneratePanelTextures(int from, int to)
        {
            EnsureDir(PanelTexDir);
            s_pending.Clear();
            var layout = Layout();
            try
            {
                for (int i = from; i < Mathf.Min(to, layout.Count); i++)
                {
                    EditorUtility.DisplayProgressBar("Concrete Life", $"Platte {i + 1}/{layout.Count}", (i - from) / (float)(to - from));
                    var p = layout[i];
                    GenerateConcrete($"{PanelTexDir}/Platte_{i:00}", 512, 1536, p.width, PanelH, p.seed, p.anchors, 5 + (p.seed % 8));
                }
                ApplyImportSettings();
            }
            finally { EditorUtility.ClearProgressBar(); }
        }

        public static void GenerateEnvironmentTextures()
        {
            EnsureDir(TexDir);
            s_pending.Clear();
            try
            {
                EditorUtility.DisplayProgressBar("Concrete Life", "Kopfbalken", 0.1f);
                float beamLen = WallRight - WallLeft + 0.4f;
                var beamAnchors = new List<Anchor>();
                var rnd = new System.Random(77);
                for (int i = 0; i < 12; i++)
                    beamAnchors.Add(new Anchor { uv = new Vector2((float)rnd.NextDouble(), 0.3f + (float)rnd.NextDouble() * 0.4f), length = 0.3f, strength = 0.5f });
                GenerateConcrete($"{TexDir}/Kopfbalken", 4096, 96, beamLen, BeamH, 4242, beamAnchors, 45);

                EditorUtility.DisplayProgressBar("Concrete Life", "Boden", 0.4f);
                GenerateGround();
                GenerateDetail();
                GenerateRipple();
                GenerateRayTexture();
                ApplyImportSettings();
            }
            finally { EditorUtility.ClearProgressBar(); }
        }

        static void GenerateConcrete(string basePath, int W, int H, float widthM, float heightM, int seedI, List<Anchor> anchors, int chipCount)
        {
            var rnd = new System.Random(seedI);
            float seed = 100f + (seedI % 997) * 3.17f;
            var col = new Color[W * H];
            var h = new float[W * H];

            var chips = new List<Vector3>();
            for (int i = 0; i < chipCount; i++)
            {
                float cx = (float)rnd.NextDouble() * widthM, cy = (float)rnd.NextDouble() * heightM;
                if (rnd.NextDouble() < 0.6)
                {
                    int side = rnd.Next(4);
                    float o = (float)rnd.NextDouble() * 0.05f;
                    if (side == 0) cx = o; else if (side == 1) cx = widthM - o; else if (side == 2) cy = o; else cy = heightM - o;
                }
                chips.Add(new Vector3(cx, cy, 0.03f + (float)rnd.NextDouble() * 0.09f));
            }

            // Farbton je Platte leicht verschieden (Betonchargen)
            float tint = (float)rnd.NextDouble();
            Color cA = Color.Lerp(new Color(0.60f, 0.59f, 0.55f), new Color(0.56f, 0.56f, 0.54f), tint);
            Color cB = Color.Lerp(new Color(0.47f, 0.47f, 0.45f), new Color(0.50f, 0.49f, 0.45f), tint);
            Color rustC = new Color(0.40f, 0.19f, 0.07f), chipC = new Color(0.68f, 0.66f, 0.61f), stainC = new Color(0.30f, 0.31f, 0.26f);
            float stainBias = (float)rnd.NextDouble() * 0.08f;

            for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                int i = y * W + x;
                float u = x / (W - 1f), v = y / (H - 1f);
                float mx = u * widthM, my = v * heightM;

                float large = Fbm(mx * 0.45f, my * 0.45f, 3, seed);
                float fine = Fbm(mx * 7f, my * 7f, 3, seed + 40f);
                float pn = Mathf.PerlinNoise(mx * 55f + seed, my * 55f + seed * 0.5f);
                float pore = pn < 0.2f ? (0.2f - pn) / 0.2f : 0f;

                float height = 0.5f + 0.3f * (fine - 0.5f) - 0.35f * pore;
                Color c = Color.Lerp(cA, cB, large) * (0.9f + 0.2f * fine);

                // Wasserflecken und lange senkrechte Schlieren
                float streak = Fbm(mx * 3f, my * 0.12f, 3, seed + 90f);
                float blot = Fbm(mx * 0.4f, my * 0.3f, 3, seed + 130f);
                float stain = Smooth(0.47f, 0.70f, blot * 0.5f + streak * 0.5f + (1f - v) * 0.08f + stainBias);
                c = Color.Lerp(c, c * 0.62f, stain);
                c = Color.Lerp(c, stainC, stain * 0.25f);
                c *= 1f - 0.35f * pore;

                // abgerundete Kanten
                float e = Mathf.Min(Mathf.Min(mx, widthM - mx), Mathf.Min(my, heightM - my));
                if (e < 0.025f) { float k = 1f - e / 0.025f; height -= k * k * 0.5f; c *= 1f - 0.15f * k; }

                // Abplatzungen
                foreach (var ch in chips)
                {
                    float dx = mx - ch.x, dy = my - ch.y;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) / ch.z;
                    if (d > 1.5f) continue;
                    d += 0.45f * (Mathf.PerlinNoise(mx * 18f + seed + ch.x * 10f, my * 18f + ch.y * 10f) - 0.5f);
                    if (d < 1f)
                    {
                        float k = Smooth(1f, 0.75f, d);
                        height -= 0.3f * k;
                        c = Color.Lerp(c, chipC * (0.85f + 0.3f * fine), k * 0.85f);
                    }
                }

                // Rost unter den Ankern
                foreach (var a in anchors)
                {
                    float bx = a.uv.x * widthM, by = a.uv.y * heightM;
                    float rx = mx - bx, ry = by - my;
                    if (Mathf.Abs(rx) > 0.3f || ry < -0.1f || ry > a.length) continue;
                    float halo = Mathf.Exp(-(rx * rx + ry * ry) / (0.05f * 0.05f));
                    float streakR = 0f;
                    if (ry > 0f)
                    {
                        float w = 0.012f + ry * 0.04f;
                        float fall = Mathf.Pow(1f - ry / a.length, 1.4f);
                        float n = 0.45f + 0.9f * Fbm(mx * 35f, my * 2.5f, 2, seed + 200f + bx);
                        streakR = Mathf.Exp(-(rx * rx) / (w * w)) * fall * n * a.strength;
                    }
                    float r = Mathf.Clamp01(Mathf.Max(halo, streakR));
                    c = Color.Lerp(c, rustC * (0.8f + 0.4f * fine), r * 0.8f);
                }

                c.a = 1f;
                col[i] = c;
                h[i] = height;
            }

            SaveTexture(basePath + "_Albedo.png", col, W, H, TexKind.Color, TextureWrapMode.Clamp);
            SaveTexture(basePath + "_Normal.png", HeightToNormal(h, W, H, 5f), W, H, TexKind.Normal, TextureWrapMode.Clamp);
        }

        /// Ein durchgehendes Bodenbild fuer die ganze sichtbare Flaeche (keine Kachelung).
        static void GenerateGround()
        {
            const int W = 4096, H = 1024;
            float seed = 500f;
            var col = new Color[W * H];
            var h = new float[W * H];
            Color sand = new Color(0.52f, 0.48f, 0.39f), mud = new Color(0.22f, 0.21f, 0.16f), pebble = new Color(0.32f, 0.31f, 0.28f);
            for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                int i = y * W + x;
                float mx = x / (W - 1f) * GroundW, my = y / (H - 1f) * GroundD;
                float m = Fbm(mx * 0.18f, my * 0.18f, 4, seed);
                float fine = Fbm(mx * 1.6f, my * 1.6f, 3, seed + 30f);
                float pebN = Mathf.PerlinNoise(mx * 4.5f + seed, my * 4.5f + 60f);
                float mudMask = Smooth(0.42f, 0.62f, m);
                Color c = Color.Lerp(sand, mud, mudMask) * (0.85f + 0.3f * fine);
                float height = 0.5f + 0.25f * (fine - 0.5f) - 0.1f * mudMask;
                float peb = Smooth(0.70f, 0.80f, pebN);
                c = Color.Lerp(c, pebble * (0.8f + 0.4f * fine), peb * 0.8f);
                height += peb * 0.15f;
                c.a = 1f;
                col[i] = c;
                h[i] = height;
            }
            SaveTexture($"{TexDir}/Ground_Albedo.png", col, W, H, TexKind.Color, TextureWrapMode.Clamp);
            SaveTexture($"{TexDir}/Ground_Normal.png", HeightToNormal(h, W, H, 3f), W, H, TexKind.Normal, TextureWrapMode.Clamp);
        }

        /// Feines Sandkorn als Detail-Map (so fein, dass die Kachelung nicht auffaellt).
        static void GenerateDetail()
        {
            const int S = 256;
            var col = new Color[S * S];
            var h = new float[S * S];
            for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                float n = SeamFbm(x / (float)S, y / (float)S, 18f, 3, 700f);
                float g = 0.5f + (n - 0.5f) * 0.5f;
                col[y * S + x] = new Color(g, g, g, 1f);
                h[y * S + x] = n;
            }
            SaveTexture($"{TexDir}/Detail_Sand_Albedo.png", col, S, S, TexKind.Color, TextureWrapMode.Repeat);
            SaveTexture($"{TexDir}/Detail_Sand_Normal.png", HeightToNormal(h, S, S, 2f), S, S, TexKind.Normal, TextureWrapMode.Repeat);
        }

        static void GenerateRipple()
        {
            const int S = 512;
            var col = new Color[S * S];
            for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                float n = Smooth(0.3f, 0.7f, SeamFbm(x / (float)S, y / (float)S, 7f, 4, 900f));
                col[y * S + x] = new Color(n, n, n, 1f);
            }
            SaveTexture($"{TexDir}/WaterRipple.png", col, S, S, TexKind.Linear, TextureWrapMode.Repeat);
        }

        static void GenerateRayTexture()
        {
            const int W = 64, H = 256;
            var col = new Color[W * H];
            for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                float u = x / (W - 1f), v = y / (H - 1f);
                float hx = Mathf.Exp(-Mathf.Pow((u - 0.5f) / 0.2f, 2f));
                float vert = Mathf.Pow(v, 1.3f) * Smooth(1f, 0.9f, v);
                float a = hx * vert * (0.75f + 0.5f * Fbm(u * 6f, v * 1.5f, 3, 77f));
                col[y * W + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
            }
            SaveTexture($"{TexDir}/GodRay.png", col, W, H, TexKind.Linear, TextureWrapMode.Clamp);
        }

        /// Caustics-Einzelbilder [from, to). Ein Bild deckt 20 x 20 m ab -> im Bild keine sichtbare Wiederholung.
        public static void GenerateCausticTextures(int from, int to)
        {
            EnsureDir(CausticDir);
            s_pending.Clear();
            const int S = CausticRes, G = CausticCells;
            var rnd = new System.Random(42);
            var basePt = new Vector2[G, G];
            var phase = new Vector2[G, G];
            for (int j = 0; j < G; j++)
            for (int i = 0; i < G; i++)
            {
                basePt[i, j] = new Vector2(0.15f + 0.7f * (float)rnd.NextDouble(), 0.15f + 0.7f * (float)rnd.NextDouble());
                phase[i, j] = new Vector2((float)rnd.NextDouble() * 6.283f, (float)rnd.NextDouble() * 6.283f);
            }
            // grossflaechige hell/dunkel-Verteilung, damit kein gleichmaessiges Netz entsteht
            var macro = new float[S * S];
            for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
                macro[y * S + x] = SeamFbm(x / (float)S, y / (float)S, 4f, 3, 333f);

            var pts = new Vector2[G, G];
            var col = new Color[S * S];
            try
            {
                for (int f = from; f < Mathf.Min(to, CausticFrames); f++)
                {
                    EditorUtility.DisplayProgressBar("Concrete Life", $"Caustics {f + 1}/{CausticFrames}", (f - from) / (float)(to - from));
                    float ang = 6.2831853f * f / CausticFrames;
                    for (int j = 0; j < G; j++)
                    for (int i = 0; i < G; i++)
                        pts[i, j] = new Vector2(i + basePt[i, j].x + 0.2f * Mathf.Cos(ang + phase[i, j].x),
                                                j + basePt[i, j].y + 0.2f * Mathf.Sin(ang + phase[i, j].y));

                    for (int y = 0; y < S; y++)
                    for (int x = 0; x < S; x++)
                    {
                        float gx = x / (float)S * G, gy = y / (float)S * G;
                        int ci = (int)gx, cj = (int)gy;
                        float f1 = 99f, f2 = 99f;
                        for (int dj = -1; dj <= 1; dj++)
                        for (int di = -1; di <= 1; di++)
                        {
                            int cx = ci + di, cy = cj + dj;
                            int wx = ((cx % G) + G) % G, wy = ((cy % G) + G) % G;
                            float px = pts[wx, wy].x + (cx - wx) - gx;
                            float py = pts[wx, wy].y + (cy - wy) - gy;
                            float d = px * px + py * py;
                            if (d < f1) { f2 = f1; f1 = d; } else if (d < f2) f2 = d;
                        }
                        float edge = Mathf.Sqrt(f2) - Mathf.Sqrt(f1);
                        // weich und schwach: Caustics in mehreren Metern Tiefe sind kaum noch scharf
                        float line = 1f - Smooth(0f, 0.45f, edge);
                        line *= line;
                        float m = Smooth(0.25f, 0.75f, macro[y * S + x]);
                        float val = Mathf.Min(1f, 0.65f + line * (0.1f + 0.25f * m));
                        col[y * S + x] = new Color(val, val, val, 1f);
                    }
                    SaveTexture($"{CausticDir}/Caustic_{f:00}.png", col, S, S, TexKind.Linear, TextureWrapMode.Repeat);
                }
                ApplyImportSettings();
            }
            finally { EditorUtility.ClearProgressBar(); }
        }

        // ------------------------------------------------------------------ Szene

        [MenuItem("Concrete Life/Slice Mauer/2 - Szene aufbauen")]
        public static void BuildScene()
        {
            var layout = Layout();
            if (!File.Exists($"{PanelTexDir}/Platte_{layout.Count - 1:00}_Albedo.png") ||
                !File.Exists($"{TexDir}/Kopfbalken_Albedo.png") ||
                !File.Exists($"{CausticDir}/Caustic_{CausticFrames - 1:00}.png"))
                GenerateTextures();

            EnsureDir(MatDir);
            EnsureDir(MatDir + "/Platten");
            EnsureDir("Assets/Scenes");
            EnsureDir("Assets/Settings");

            var active = EditorSceneManager.GetActiveScene();
            if (active.isDirty && !string.IsNullOrEmpty(active.path) && active.path != ScenePath) EditorSceneManager.SaveScene(active);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Materialien
            var backingMat = LitMaterial("Fugen_Dunkel", new Color(0.09f, 0.09f, 0.08f), null, null, Vector2.one, 0.1f);
            var rustMat = LitMaterial("Anker_Rost", new Color(0.28f, 0.14f, 0.06f), null, null, Vector2.one, 0.3f, 0.4f);
            var beamMat = LitMaterial("Kopfbalken", Color.white, Tex("Kopfbalken_Albedo"), Tex("Kopfbalken_Normal"), Vector2.one, 0.22f);
            var groundMat = LitMaterial("Boden_Schlamm", Color.white, Tex("Ground_Albedo"), Tex("Ground_Normal"), Vector2.one, 0.15f);
            AddDetail(groundMat, new Vector2(GroundW, GroundD));
            var rockMat = LitMaterial("Stein", new Color(0.55f, 0.54f, 0.5f), null, null, Vector2.one, 0.2f);
            AddDetail(rockMat, new Vector2(2f, 2f));

            var waterMat = CustomMaterial("Wasseroberflaeche", "ConcreteLife/WaterSurface");
            waterMat.SetTexture("_RippleTex", Tex("WaterRipple"));
            waterMat.SetColor("_ShallowColor", new Color(0.32f, 0.55f, 0.5f));
            waterMat.SetColor("_BrightColor", new Color(0.85f, 1.25f, 1.15f));
            waterMat.SetColor("_ReflectColor", new Color(0.16f, 0.30f, 0.27f));
            waterMat.SetFloat("_WindowCos", 0.66f);
            waterMat.SetVector("_SunDir", SunViewDir);
            waterMat.SetColor("_SunColor", new Color(9f, 8.5f, 7f));
            waterMat.SetFloat("_Scale", 0.06f);
            waterMat.SetFloat("_Speed", 0.02f);

            // Unterwasser-Dunst + Tiefenverlauf als Bildschirm-Effekt (nur fuer den Teil unter Wasser)
            var depthMat = CustomMaterial("Tiefenverlauf", "ConcreteLife/UnderwaterView");
            depthMat.SetColor("_FogColor", FogColor);
            depthMat.SetFloat("_Density", 0.06f);
            depthMat.SetFloat("_WaterLevel", WaterLevel);
            depthMat.SetColor("_TopColor", new Color(0.55f, 0.85f, 0.8f, 0.16f));
            depthMat.SetColor("_BottomColor", new Color(0.02f, 0.07f, 0.07f, 0.5f));
            depthMat.SetFloat("_TopStart", 0.55f);
            depthMat.SetFloat("_BottomEnd", 0.5f);

            var skyMat = CustomMaterial("Himmel", "ConcreteLife/SkyDome");
            skyMat.SetVector("_SunDir", SunViewDir);
            var quayMat = LitMaterial("Kai_Oben", new Color(0.62f, 0.6f, 0.56f), null, null, Vector2.one, 0.2f);
            AddDetail(quayMat, new Vector2(60f, 30f));

            var rayMat = CustomMaterial("Lichtstrahl", "ConcreteLife/AdditiveRay");
            rayMat.SetTexture("_BaseMap", Tex("GodRay"));
            rayMat.SetFloat("_FadeDistance", 2f);
            // Grundwert, solange GodRaySway noch keine eigene Farbe gesetzt hat (z. B. im Editor)
            rayMat.SetColor("_Color", new Color(0.3f, 0.38f, 0.34f, 1f));

            var particleMat = CustomMaterial("Schwebeteilchen", "ConcreteLife/SoftParticle");

            // --- Atmosphaere / Licht-Einstellungen
            // Unity-Nebel aus: der Dunst kommt aus "UnderwaterView" und gilt nur unter Wasser
            RenderSettings.fog = false;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.06f;
            RenderSettings.fogColor = FogColor;
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.45f, 0.58f, 0.52f);
            RenderSettings.ambientEquatorColor = new Color(0.28f, 0.38f, 0.33f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.15f, 0.12f);
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.reflectionIntensity = 0.25f;

            var env = new GameObject("Umgebung").transform;
            var lighting = new GameObject("Licht").transform;
            var atmo = new GameObject("Atmosphaere").transform;

            // --- Kamera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            cam.fieldOfView = CamFov;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 250f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = FogColor;
            camGo.transform.SetPositionAndRotation(CamPos, Quaternion.Euler(CamPitch, 0f, 0f));

            // Unterwasser-Ansicht (Dunst + Tiefenverlauf) als Quad direkt vor der Kamera
            var depthQuad = Primitive(PrimitiveType.Quad, "Unterwasser_Ansicht", camGo.transform, depthMat, keepCollider: false);
            depthQuad.transform.localPosition = new Vector3(0f, 0f, 0.15f);
            depthQuad.transform.localRotation = Quaternion.identity;
            depthQuad.transform.localScale = new Vector3(1f, 0.5f, 1f);
            NoShadows(depthQuad);
            var camData = cam.GetUniversalAdditionalCameraData();
            camData.renderPostProcessing = true;
            camData.requiresDepthTexture = true; // fuer weiche Lichtstrahlen
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;

            // --- Mauer: hochkant stehende Fertigteile (jedes mit eigener Textur) + Kopfbalken
            var wall = new GameObject("Mauer_Heute").transform;
            wall.SetParent(env);
            float wallW = WallRight - WallLeft;
            var backing = Primitive(PrimitiveType.Cube, "Fugen_Hintergrund", wall, backingMat);
            backing.transform.localPosition = new Vector3(0f, PanelH * 0.5f, 0.4f);
            backing.transform.localScale = new Vector3(wallW + 2f, PanelH + 0.4f, 0.2f);

            foreach (var p in layout)
            {
                var panel = new GameObject($"Fertigteil_{p.index:00}").transform;
                panel.SetParent(wall);
                panel.localPosition = new Vector3(p.x, PanelH * 0.5f, p.zOffset);
                panel.localRotation = Quaternion.Euler(0f, p.rotY, p.rotZ);

                var mat = LitMaterial($"Platten/Platte_{p.index:00}", Color.white,
                    AssetDatabase.LoadAssetAtPath<Texture2D>($"{PanelTexDir}/Platte_{p.index:00}_Albedo.png"),
                    AssetDatabase.LoadAssetAtPath<Texture2D>($"{PanelTexDir}/Platte_{p.index:00}_Normal.png"),
                    Vector2.one, 0.22f);
                var body = Primitive(PrimitiveType.Cube, "Beton", panel, mat);
                body.transform.localScale = new Vector3(p.width, PanelH, 0.6f);

                foreach (var a in p.anchors)
                {
                    var bolt = Primitive(PrimitiveType.Cylinder, "Anker", panel, rustMat, keepCollider: false);
                    bolt.transform.localPosition = new Vector3((a.uv.x - 0.5f) * p.width, (a.uv.y - 0.5f) * PanelH, -0.3f);
                    bolt.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    bolt.transform.localScale = new Vector3(0.07f, 0.015f, 0.07f);
                }
            }

            var beam = Primitive(PrimitiveType.Cube, "Kopfbalken", wall, beamMat);
            beam.transform.localPosition = new Vector3(0f, PanelH + BeamH * 0.5f, -0.1f);
            beam.transform.localScale = new Vector3(wallW + 0.4f, BeamH, BeamDepth);

            // --- Boden (eine durchgehende Flaeche vor der Mauer)
            var ground = Primitive(PrimitiveType.Plane, "Boden", env, groundMat);
            ground.transform.localPosition = new Vector3(0f, 0f, -GroundD * 0.5f + 0.5f);
            ground.transform.localScale = new Vector3(GroundW / 10f, 1f, GroundD / 10f);

            var rnd = new System.Random(7);
            var rocks = new GameObject("Steine").transform;
            rocks.SetParent(env);
            for (int i = 0; i < 18; i++)
            {
                var rock = Primitive(PrimitiveType.Sphere, "Stein", rocks, rockMat);
                float s = 0.4f + (float)rnd.NextDouble() * 0.5f;
                rock.transform.localPosition = new Vector3(WallLeft + 2f + (float)rnd.NextDouble() * (WallRight - WallLeft - 4f), 0.03f, -0.45f - (float)rnd.NextDouble() * 0.6f);
                rock.transform.localScale = new Vector3(s, s * (0.35f + (float)rnd.NextDouble() * 0.3f), s * 0.8f);
                rock.transform.localRotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);
            }

            // --- Wasseroberflaeche (von unten gesehen)
            var surface = Primitive(PrimitiveType.Plane, "Wasseroberflaeche", env, waterMat, keepCollider: false);
            surface.transform.localPosition = new Vector3(0f, WaterLevel, -13f);
            surface.transform.localScale = new Vector3(10f, 1f, 3.2f);
            NoShadows(surface);

            // --- Ueber Wasser: Himmel und Kai-Flaeche hinter der Mauer (Skyline folgt spaeter)
            var above = new GameObject("Ueber_Wasser").transform;
            above.SetParent(env);
            var sky = Primitive(PrimitiveType.Sphere, "Himmel", above, skyMat, keepCollider: false);
            sky.transform.localPosition = Vector3.zero;
            sky.transform.localScale = Vector3.one * 400f;
            NoShadows(sky);
            var quay = Primitive(PrimitiveType.Plane, "Kai", above, quayMat);
            quay.transform.localPosition = new Vector3(0f, PanelH + BeamH, 0.4f + 20f);
            quay.transform.localScale = new Vector3(10f, 1f, 4f);

            // --- Sonne mit Caustics
            var sunGo = new GameObject("Sonne_Caustics");
            sunGo.transform.SetParent(lighting);
            // Beleuchtung der Mauer kommt steil von oben (gestreutes Licht im Wasser);
            // die sichtbare Sonne + Strahlen liegen davon unabhaengig vor der Kamera.
            sunGo.transform.rotation = Quaternion.Euler(62f, -15f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.86f, 1f, 0.92f);
            sun.intensity = 2.2f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.75f;
            var frames = Enumerable.Range(0, CausticFrames)
                .Select(f => AssetDatabase.LoadAssetAtPath<Texture2D>($"{CausticDir}/Caustic_{f:00}.png"))
                .Where(t => t != null).ToArray();
            if (frames.Length > 0) sun.cookie = frames[0];
            var sunData = sun.GetUniversalAdditionalLightData();
            sunData.lightCookieSize = new Vector2(CausticWorldSize, CausticWorldSize);
            var caustics = sunGo.AddComponent<CausticsAnimator>();
            caustics.frames = frames;
            caustics.framesPerSecond = 8f;

            // --- Lichtstrahlen: viele schwache, weiche Strahlen, alle parallel zur Sonnenrichtung.
            // Weil die Kamera Richtung Sonne schaut, laufen sie perspektivisch auf die Sonne zu (Faecher).
            var rays = new GameObject("Lichtstrahlen").transform;
            rays.SetParent(lighting);
            // Faecher: alle Strahlen beginnen nahe dem Punkt, an dem man die Sonne durch die Oberflaeche
            // sieht, und laufen schraeg nach unten auseinander bis zum Boden vor der Mauer.
            // (Kuenstlerische Vereinfachung des perspektivischen Effekts aus dem Kelp-Beispiel.)
            Vector3 sunPoint = CamPos + SunViewDir * ((WaterLevel - CamPos.y) / SunViewDir.y);
            for (int i = 0; i < 34; i++)
            {
                var start = sunPoint + new Vector3(((float)rnd.NextDouble() - 0.5f) * 3f, 0f, ((float)rnd.NextDouble() - 0.5f) * 1.2f);
                start.z = Mathf.Min(start.z, -0.9f);
                float gx = (float)(rnd.NextDouble() + rnd.NextDouble() - 1.0); // zur Mitte gehaeuft
                var target = new Vector3(gx * 11f, 0f, -1.0f - (float)rnd.NextDouble() * 5.5f);
                Vector3 axis = (start - target).normalized; // zeigt nach oben zur Oberflaeche
                float len = Vector3.Distance(start, target);
                var center = (start + target) * 0.5f;
                var ray = Primitive(PrimitiveType.Quad, $"Strahl_{i:00}", rays, rayMat, keepCollider: false);
                Vector3 toCam = Vector3.ProjectOnPlane(CamPos - center, axis).normalized;
                ray.transform.SetPositionAndRotation(center, Quaternion.LookRotation(toCam, axis));
                ray.transform.localScale = new Vector3(0.25f + (float)rnd.NextDouble() * 1.0f, len, 1f);
                NoShadows(ray);
                var sway = ray.AddComponent<GodRaySway>();
                float k = 0.5f + (float)rnd.NextDouble() * 0.6f; // etwas heller, weil der Dunst sie jetzt mit abschwaecht
                sway.baseColor = new Color(0.75f * k, 0.95f * k, 0.85f * k, 1f);
                sway.intensityVariation = 0.8f;
                sway.speed = 0.1f + (float)rnd.NextDouble() * 0.15f;
                sway.swayAngle = 0.8f;
            }

            // --- Schwebeteilchen
            var psGo = new GameObject("Schwebeteilchen");
            psGo.transform.SetParent(atmo);
            psGo.transform.position = new Vector3(0f, (WaterLevel - 0.3f) * 0.5f, -5.5f);
            var ps = psGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 10f;
            main.loop = true;
            main.prewarm = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(10f, 16f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.05f);
            main.startColor = new Color(0.85f, 0.95f, 0.9f, 0.55f);
            main.maxParticles = 800;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 52f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(30f, WaterLevel - 0.3f, 11f);
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.15f;
            noise.frequency = 0.25f;
            noise.scrollSpeed = 0.05f;
            var colLife = ps.colorOverLifetime;
            colLife.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
            colLife.color = grad;
            var psr = psGo.GetComponent<ParticleSystemRenderer>();
            psr.sharedMaterial = particleMat;
            psr.renderMode = ParticleSystemRenderMode.Billboard;
            psr.shadowCastingMode = ShadowCastingMode.Off;
            psr.receiveShadows = false;

            // --- Post-Processing
            var volGo = new GameObject("Global Volume");
            volGo.transform.SetParent(atmo);
            var vol = volGo.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.sharedProfile = CreateVolumeProfile();

            EditorSceneManager.SaveScene(scene, ScenePath);
            SliceMauerDetails.Build(); // Pflanzen, Muell, Fische
            SliceMauerPlayer.Build();  // Taucherin, Eingabe, Kamerafuehrung
            AddSceneToBuild(ScenePath);
            AssetDatabase.SaveAssets();

            Selection.activeGameObject = camGo;
            if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.AlignViewToObject(camGo.transform);
            Debug.Log("[Concrete Life] Szene aufgebaut: " + ScenePath);
        }

        static VolumeProfile CreateVolumeProfile()
        {
            if (AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumePath) != null) AssetDatabase.DeleteAsset(VolumePath);
            var p = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(p, VolumePath);

            var tm = AddOverride<Tonemapping>(p);
            tm.mode.value = TonemappingMode.ACES;

            var ca = AddOverride<ColorAdjustments>(p);
            ca.postExposure.value = 0.5f;
            ca.contrast.value = 14f;
            ca.saturation.value = -12f;
            ca.colorFilter.value = new Color(0.93f, 1f, 0.95f);

            var bloom = AddOverride<Bloom>(p);
            bloom.threshold.value = 0.95f;
            bloom.intensity.value = 0.6f;
            bloom.scatter.value = 0.65f;

            var vig = AddOverride<Vignette>(p);
            vig.intensity.value = 0.38f;
            vig.smoothness.value = 0.45f;

            var dof = AddOverride<DepthOfField>(p);
            dof.mode.value = DepthOfFieldMode.Gaussian;
            dof.gaussianStart.value = 14f;
            dof.gaussianEnd.value = 38f;
            dof.gaussianMaxRadius.value = 1.2f;
            dof.highQualitySampling.value = true;

            var grain = AddOverride<FilmGrain>(p);
            grain.type.value = FilmGrainLookup.Thin1;
            grain.intensity.value = 0.2f;
            grain.response.value = 0.8f;

            EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            return p;
        }

        static T AddOverride<T>(VolumeProfile p) where T : VolumeComponent
        {
            var c = p.Add<T>(true);
            c.name = typeof(T).Name;
            AssetDatabase.AddObjectToAsset(c, p);
            return c;
        }

        // ------------------------------------------------------------------ Helfer

        static GameObject Primitive(PrimitiveType type, string name, Transform parent, Material mat, bool keepCollider = true)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static void NoShadows(GameObject go)
        {
            var r = go.GetComponent<Renderer>();
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        static Texture2D Tex(string name) => AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexDir}/{name}.png");

        static void AddDetail(Material m, Vector2 tiling)
        {
            m.SetTexture("_DetailAlbedoMap", Tex("Detail_Sand_Albedo"));
            m.SetTexture("_DetailNormalMap", Tex("Detail_Sand_Normal"));
            m.SetFloat("_DetailAlbedoMapScale", 1f);
            m.SetFloat("_DetailNormalMapScale", 0.6f);
            m.SetTextureScale("_DetailAlbedoMap", tiling);
            m.EnableKeyword("_DETAIL_MULX2");
            EditorUtility.SetDirty(m);
        }

        static Material LitMaterial(string name, Color color, Texture2D albedo, Texture2D normal, Vector2 tiling, float smoothness, float metallic = 0f)
        {
            var m = GetOrCreateMaterial(name, "Universal Render Pipeline/Lit");
            m.SetColor("_BaseColor", color);
            m.SetTexture("_BaseMap", albedo);
            m.SetTextureScale("_BaseMap", tiling);
            if (normal != null)
            {
                m.SetTexture("_BumpMap", normal);
                m.SetFloat("_BumpScale", 1f);
                m.EnableKeyword("_NORMALMAP");
            }
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material CustomMaterial(string name, string shader)
        {
            var m = GetOrCreateMaterial(name, shader);
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material GetOrCreateMaterial(string name, string shaderName)
        {
            string path = $"{MatDir}/{name}.mat";
            var shader = Shader.Find(shaderName);
            if (shader == null) Debug.LogError("[Concrete Life] Shader nicht gefunden: " + shaderName);
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            else if (m.shader != shader) m.shader = shader;
            return m;
        }

        static void AddSceneToBuild(string path)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == path)) return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void EnsureDir(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureDir(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static void SaveTexture(string path, Color[] px, int w, int h, TexKind kind, TextureWrapMode wrap)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            s_pending.Add(new PendingTex { path = path, kind = kind, wrap = wrap, size = Mathf.Max(w, h) });
        }

        static void ApplyImportSettings()
        {
            AssetDatabase.Refresh();
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var p in s_pending)
                {
                    var imp = AssetImporter.GetAtPath(p.path) as TextureImporter;
                    if (imp == null) continue;
                    imp.textureType = p.kind == TexKind.Normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                    if (p.kind != TexKind.Normal) imp.sRGBTexture = p.kind == TexKind.Color;
                    imp.alphaIsTransparency = false;
                    imp.wrapMode = p.wrap;
                    imp.mipmapEnabled = true;
                    imp.maxTextureSize = Mathf.Min(4096, Mathf.NextPowerOfTwo(p.size));
                    imp.SaveAndReimport();
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            s_pending.Clear();
        }

        static Color[] HeightToNormal(float[] h, int w, int hgt, float strength)
        {
            var n = new Color[w * hgt];
            for (int y = 0; y < hgt; y++)
            for (int x = 0; x < w; x++)
            {
                int xl = Mathf.Max(x - 1, 0), xr = Mathf.Min(x + 1, w - 1);
                int yd = Mathf.Max(y - 1, 0), yu = Mathf.Min(y + 1, hgt - 1);
                float dx = h[y * w + xr] - h[y * w + xl];
                float dy = h[yu * w + x] - h[yd * w + x];
                var v = new Vector3(-dx * strength, -dy * strength, 1f).normalized;
                n[y * w + x] = new Color(v.x * 0.5f + 0.5f, v.y * 0.5f + 0.5f, v.z * 0.5f + 0.5f, 1f);
            }
            return n;
        }

        static float Fbm(float x, float y, int octaves, float seed)
        {
            float sum = 0f, amp = 0.5f, norm = 0f, f = 1f;
            for (int i = 0; i < octaves; i++)
            {
                sum += amp * Mathf.PerlinNoise(x * f + seed + i * 19.17f, y * f + seed * 0.73f + i * 7.31f);
                norm += amp;
                amp *= 0.5f;
                f *= 2.03f;
            }
            return sum / norm;
        }

        // Kachelbares Rauschen (u, v in 0..1)
        static float SeamFbm(float u, float v, float scale, int octaves, float seed)
        {
            float a = Fbm(u * scale, v * scale, octaves, seed);
            float b = Fbm((u - 1f) * scale, v * scale, octaves, seed);
            float c = Fbm(u * scale, (v - 1f) * scale, octaves, seed);
            float d = Fbm((u - 1f) * scale, (v - 1f) * scale, octaves, seed);
            return a * (1 - u) * (1 - v) + b * u * (1 - v) + c * (1 - u) * v + d * u * v;
        }

        static float Smooth(float e0, float e1, float x)
        {
            float t = Mathf.Clamp01((x - e0) / (e1 - e0));
            return t * t * (3f - 2f * t);
        }
    }
}
