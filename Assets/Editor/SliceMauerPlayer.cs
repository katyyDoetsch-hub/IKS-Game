using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ConcreteLife.EditorTools
{
    /// <summary>
    /// Taucherin, Eingabe und Kamerafuehrung fuer die Szene "Slice_Mauer".
    /// Das Prefab "Taucherin" wird nur angelegt, wenn es noch nicht existiert (Katjas Modell bleibt erhalten).
    /// Austausch: im Prefab das Kind "Modell/Platzhalter" loeschen und das Blender-Modell unter "Modell" setzen
    /// (Kopf entlang +Z, Ruecken +Y, Pivot Koerpermitte).
    /// </summary>
    public static class SliceMauerPlayer
    {
        const string PrefabDir = "Assets/Prefabs";
        const string PrefabPath = PrefabDir + "/Taucherin.prefab";
        const string MatDir = "Assets/Materials/Slice_Mauer/Taucherin";

        static readonly Vector3 StartPos = new Vector3(0f, 3.2f, -1.8f);

        [MenuItem("Concrete Life/Slice Mauer/4 - Taucherin und Steuerung")]
        public static void Build()
        {
            EnsureDir(PrefabDir);
            EnsureDir(MatDir);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) prefab = CreateDiverPrefab();

            foreach (var n in new[] { "Taucherin", "Eingabe" })
            {
                var old = GameObject.Find(n);
                if (old) Object.DestroyImmediate(old);
            }

            var input = new GameObject("Eingabe");
            input.AddComponent<PlayerInput>();
            input.AddComponent<DevKeys>();

            var diver = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            diver.name = "Taucherin";
            diver.transform.position = StartPos;
            diver.transform.rotation = Quaternion.Euler(0f, 115f, 0f);

            var cam = Camera.main;
            if (cam != null)
            {
                var follow = cam.GetComponent<CameraFollow>();
                if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
                follow.target = diver.transform;
                follow.waterLevel = SliceMauerBuilder.WaterLevel;
                var surf = GameObject.Find("Wasseroberflaeche");
                if (surf) follow.waterSurface = surf.GetComponent<Renderer>();
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Concrete Life] Taucherin und Steuerung eingebaut.");
        }

        static GameObject CreateDiverPrefab()
        {
            var suit = Lit("Anzug", new Color(0.08f, 0.09f, 0.1f), 0.35f);
            var orange = Lit("Akzent_Orange", new Color(0.95f, 0.42f, 0.08f), 0.4f);
            var tank = Lit("Flasche", new Color(0.42f, 0.44f, 0.47f), 0.6f, 0.6f);
            var glass = Lit("Maske", new Color(0.25f, 0.45f, 0.5f), 0.9f);

            var root = new GameObject("Taucherin");
            var ctrl = root.AddComponent<DiverController>();

            var model = new GameObject("Modell").transform;
            model.SetParent(root.transform, false);
            model.localScale = Vector3.one * 1.3f;
            ctrl.visual = model;

            var ph = new GameObject("Platzhalter");
            ph.transform.SetParent(model, false);

            // Koerper liegt entlang +Z (Kopf vorne), Ruecken nach +Y
            Part(ph, PrimitiveType.Capsule, "Koerper", suit, new Vector3(0, 0, 0.1f), new Vector3(90, 0, 0), new Vector3(0.38f, 0.6f, 0.3f));
            Part(ph, PrimitiveType.Cube, "Streifen_L", orange, new Vector3(-0.19f, 0f, 0.15f), Vector3.zero, new Vector3(0.02f, 0.06f, 0.7f));
            Part(ph, PrimitiveType.Cube, "Streifen_R", orange, new Vector3(0.19f, 0f, 0.15f), Vector3.zero, new Vector3(0.02f, 0.06f, 0.7f));
            Part(ph, PrimitiveType.Sphere, "Kopf", suit, new Vector3(0, 0.03f, 0.78f), Vector3.zero, new Vector3(0.24f, 0.24f, 0.26f));
            Part(ph, PrimitiveType.Cube, "Maske", glass, new Vector3(0, -0.02f, 0.9f), new Vector3(20, 0, 0), new Vector3(0.17f, 0.1f, 0.05f));
            Part(ph, PrimitiveType.Cylinder, "Pressluftflasche", tank, new Vector3(0, 0.2f, 0.15f), new Vector3(90, 0, 0), new Vector3(0.17f, 0.3f, 0.17f));
            Part(ph, PrimitiveType.Capsule, "Arm_L", suit, new Vector3(-0.24f, -0.03f, 0.45f), new Vector3(90, 0, 0), new Vector3(0.09f, 0.28f, 0.09f));
            Part(ph, PrimitiveType.Capsule, "Arm_R", suit, new Vector3(0.24f, -0.03f, 0.45f), new Vector3(90, 0, 0), new Vector3(0.09f, 0.28f, 0.09f));
            Part(ph, PrimitiveType.Capsule, "Bein_L", suit, new Vector3(-0.1f, -0.02f, -0.62f), new Vector3(90, 0, 0), new Vector3(0.13f, 0.32f, 0.13f));
            Part(ph, PrimitiveType.Capsule, "Bein_R", suit, new Vector3(0.1f, -0.02f, -0.62f), new Vector3(90, 0, 0), new Vector3(0.13f, 0.32f, 0.13f));

            var finL = new GameObject("Flosse_L").transform;
            finL.SetParent(ph.transform, false);
            finL.localPosition = new Vector3(-0.1f, -0.02f, -0.92f);
            Part(finL.gameObject, PrimitiveType.Cube, "Blatt", orange, new Vector3(0, 0, -0.18f), Vector3.zero, new Vector3(0.2f, 0.02f, 0.36f));
            var finR = new GameObject("Flosse_R").transform;
            finR.SetParent(ph.transform, false);
            finR.localPosition = new Vector3(0.1f, -0.02f, -0.92f);
            Part(finR.gameObject, PrimitiveType.Cube, "Blatt", orange, new Vector3(0, 0, -0.18f), Vector3.zero, new Vector3(0.2f, 0.02f, 0.36f));

            var kick = ph.AddComponent<PlaceholderFinKick>();
            kick.finLeft = finL;
            kick.finRight = finR;
            foreach (var c in ph.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);

            // Kontur (haengt sich beim Start an alle Modellteile)
            var rimMat = AssetDatabase.LoadAssetAtPath<Material>($"{MatDir}/Kontur.mat");
            if (rimMat == null)
            {
                rimMat = new Material(Shader.Find("ConcreteLife/RimLight"));
                AssetDatabase.CreateAsset(rimMat, $"{MatDir}/Kontur.mat");
            }
            rimMat.SetColor("_RimColor", new Color(0.55f, 1.0f, 0.95f) * 2.6f);
            rimMat.SetFloat("_RimPower", 3f);
            EditorUtility.SetDirty(rimMat);
            root.AddComponent<RimOutline>().rimMaterial = rimMat;

            // kleines eigenes Licht am Kopf
            var lightGo = new GameObject("Kopflicht");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.25f, 0.75f);
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.86f, 0.66f);
            l.intensity = 1.6f;
            l.range = 3f;
            l.shadows = LightShadows.None;

            // Luftblasen
            var bubbles = new GameObject("Blasen");
            bubbles.transform.SetParent(root.transform, false);
            bubbles.transform.localPosition = new Vector3(0f, 0.15f, 0.85f);
            var ps = bubbles.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.duration = 3.6f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 4.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.05f);
            main.startColor = new Color(0.85f, 1f, 0.95f, 0.8f);
            main.gravityModifier = -0.12f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 80;
            var em = ps.emission;
            em.rateOverTime = 0.8f;
            em.SetBursts(new[] { new ParticleSystem.Burst(0.2f, new ParticleSystem.MinMaxCurve(5, 9), 1, 0.06f) });
            var sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Sphere;
            sh.radius = 0.04f;
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.25f;
            noise.frequency = 0.8f;
            var bubbleMat = AssetDatabase.LoadAssetAtPath<Material>($"{MatDir}/Blasen.mat");
            if (bubbleMat == null)
            {
                bubbleMat = new Material(Shader.Find("ConcreteLife/SoftParticle"));
                AssetDatabase.CreateAsset(bubbleMat, $"{MatDir}/Blasen.mat");
            }
            bubbleMat.SetColor("_Color", new Color(1.4f, 1.6f, 1.5f, 1f));
            var psr = bubbles.GetComponent<ParticleSystemRenderer>();
            psr.sharedMaterial = bubbleMat;
            psr.shadowCastingMode = ShadowCastingMode.Off;

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static GameObject Part(GameObject parent, PrimitiveType type, string name, Material mat, Vector3 pos, Vector3 euler, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = pos;
            go.transform.localEulerAngles = euler;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        static Material Lit(string name, Color color, float smoothness, float metallic = 0f)
        {
            string path = $"{MatDir}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(m, path);
            }
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(m);
            return m;
        }

        static void EnsureDir(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureDir(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
