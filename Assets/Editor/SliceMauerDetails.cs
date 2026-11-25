using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ConcreteLife.EditorTools
{
    /// <summary>
    /// Pflanzen, Muell und Fischschwarm fuer die Szene "Slice_Mauer".
    /// Jede Art ist ein Prefab unter Assets/Prefabs/Platzhalter. Diese Prefabs werden NUR angelegt,
    /// wenn sie noch nicht existieren – so bleiben spaeter eingesetzte Blender-Modelle erhalten.
    /// </summary>
    public static class SliceMauerDetails
    {
        const string PrefabDir = "Assets/Prefabs/Platzhalter";
        const string MatDir = "Assets/Materials/Slice_Mauer/Platzhalter";

        [MenuItem("Concrete Life/Slice Mauer/3 - Pflanzen, Muell, Fische")]
        public static void Build()
        {
            EnsureDir(PrefabDir);
            EnsureDir(MatDir);

            var ecklonia = GetOrCreatePrefab("Ecklonia", BuildEcklonia);
            var sargassum = GetOrCreatePrefab("Sargassum", BuildSargassum);
            var seegras = GetOrCreatePrefab("Seegras", BuildSeegras);
            var rotalge = GetOrCreatePrefab("Rotalge", BuildRotalge);
            var flasche = GetOrCreatePrefab("Muell_Flasche", BuildFlasche);
            var dose = GetOrCreatePrefab("Muell_Dose", BuildDose);
            var seil = GetOrCreatePrefab("Muell_Seil", BuildSeil);
            var netz = GetOrCreatePrefab("Muell_Netzrest", BuildNetz);
            var fischKlein = GetOrCreatePrefab("Fisch_Klein", BuildFischKlein);
            var fischGross = GetOrCreatePrefab("Fisch_Gross", BuildFischGross);

            var env = GameObject.Find("Umgebung");
            var atmo = GameObject.Find("Atmosphaere");
            if (env == null || atmo == null)
            {
                Debug.LogError("[Concrete Life] Erst die Szene aufbauen (Schritt 2).");
                return;
            }
            foreach (var n in new[] { "Pflanzen", "Muell" })
            {
                var old = env.transform.Find(n);
                if (old) Object.DestroyImmediate(old.gameObject);
            }
            var oldFish = atmo.transform.Find("Fischschwarm");
            if (oldFish) Object.DestroyImmediate(oldFish.gameObject);

            var rnd = new System.Random(2026);
            float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);

            // --- Pflanzen: vereinzelt, nur am Rand. Mauermitte bleibt frei.
            var plants = new GameObject("Pflanzen").transform;
            plants.SetParent(env.transform);

            // Kamera steht bei z = -11; sichtbarer Boden beginnt ca. bei z = -6.5.
            // Vordergrund unten (nah an der Kamera, ragt von unten ins Bild)
            Place(seegras, plants, new Vector3(R(-4.0f, -3.2f), 0f, R(-6.2f, -5.6f)), R(1.4f, 1.7f), rnd);
            Place(rotalge, plants, new Vector3(R(-2.2f, -1.6f), 0f, R(-6.0f, -5.4f)), R(1.6f, 2.0f), rnd);
            Place(seegras, plants, new Vector3(R(3.0f, 3.8f), 0f, R(-6.2f, -5.6f)), R(1.3f, 1.6f), rnd);

            // seitlich am Bildrand (gross, rahmt das Bild)
            Place(ecklonia, plants, new Vector3(R(-7.6f, -6.8f), 0f, R(-3.6f, -2.8f)), R(1.8f, 2.1f), rnd);
            Place(sargassum, plants, new Vector3(R(-6.2f, -5.4f), 0f, R(-2.2f, -1.4f)), R(1.6f, 1.9f), rnd);
            Place(ecklonia, plants, new Vector3(R(6.8f, 7.6f), 0f, R(-3.3f, -2.5f)), R(1.7f, 2.0f), rnd);
            Place(seegras, plants, new Vector3(R(5.6f, 6.4f), 0f, R(-4.4f, -3.6f)), R(1.4f, 1.7f), rnd);

            // am Mauerfuss, vereinzelt
            Place(rotalge, plants, new Vector3(R(-3.6f, -2.8f), 0f, R(-0.9f, -0.6f)), R(1.3f, 1.6f), rnd);
            Place(sargassum, plants, new Vector3(R(2.4f, 3.2f), 0f, R(-0.9f, -0.6f)), R(1.2f, 1.5f), rnd);

            // im Hintergrund (seitlich am Mauerfuss weiter aussen, im Dunst)
            Place(ecklonia, plants, new Vector3(R(-11.5f, -9.5f), 0f, R(-1.0f, -0.6f)), R(1.5f, 1.9f), rnd);
            Place(sargassum, plants, new Vector3(R(9.0f, 10.5f), 0f, R(-1.0f, -0.6f)), R(1.4f, 1.7f), rnd);
            Place(ecklonia, plants, new Vector3(R(11.5f, 13.0f), 0f, R(-1.2f, -0.7f)), R(1.6f, 2.0f), rnd);

            // --- Muell: wenig (4 Teile), liegt im Schlamm
            var trash = new GameObject("Muell").transform;
            trash.SetParent(env.transform);
            PlaceLying(flasche, trash, new Vector3(R(-2.2f, -1.2f), 0.05f, R(-2.0f, -1.4f)), rnd);
            PlaceLying(dose, trash, new Vector3(R(1.4f, 2.4f), 0.03f, R(-2.4f, -1.8f)), rnd);
            PlaceLying(seil, trash, new Vector3(R(-0.6f, 0.6f), 0.02f, R(-1.3f, -0.9f)), rnd);
            PlaceLying(netz, trash, new Vector3(R(5.0f, 6.0f), 0.01f, R(-1.4f, -0.9f)), rnd);

            // --- Fischschwarm
            var schoolGo = new GameObject("Fischschwarm");
            schoolGo.transform.SetParent(atmo.transform);
            var school = schoolGo.AddComponent<FishSchool>();
            school.smallFishPrefab = fischKlein;
            school.bigFishPrefab = fischGross;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Concrete Life] Pflanzen, Muell und Fischschwarm platziert.");
        }

        // ------------------------------------------------------------------ Platzieren

        static void Place(GameObject prefab, Transform parent, Vector3 pos, float scale, System.Random rnd)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(((float)rnd.NextDouble() - 0.5f) * 8f, (float)rnd.NextDouble() * 360f, ((float)rnd.NextDouble() - 0.5f) * 8f);
            // nicht gleichmaessig skalieren -> jede Pflanze hat eigene Proportionen
            go.transform.localScale = new Vector3(scale * (0.85f + (float)rnd.NextDouble() * 0.3f), scale, scale * (0.85f + (float)rnd.NextDouble() * 0.3f));
            var sway = go.GetComponent<PlantSway>();
            if (sway != null)
            {
                sway.amplitude *= 0.8f + (float)rnd.NextDouble() * 0.5f;
                sway.speed *= 0.8f + (float)rnd.NextDouble() * 0.5f;
            }
        }

        static void PlaceLying(GameObject prefab, Transform parent, Vector3 pos, System.Random rnd)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(((float)rnd.NextDouble() - 0.5f) * 10f, (float)rnd.NextDouble() * 360f, ((float)rnd.NextDouble() - 0.5f) * 10f);
        }

        // ------------------------------------------------------------------ Platzhalter-Formen
        // Pivot jeweils am Fusspunkt (Pflanzen) bzw. in der Mitte (Muell, Fische). Fische schauen entlang +Z.

        static void BuildEcklonia(GameObject root)
        {
            var stem = Lit("Ecklonia_Stiel", new Color(0.35f, 0.25f, 0.1f), 0.3f);
            var blade = Lit("Ecklonia_Blatt", new Color(0.52f, 0.38f, 0.13f), 0.35f);
            Part(root, PrimitiveType.Cylinder, "Stiel", stem, new Vector3(0, 0.3f, 0), Vector3.zero, new Vector3(0.04f, 0.3f, 0.04f));
            var rnd = new System.Random(1);
            for (int i = 0; i < 6; i++)
            {
                float ang = i * 60f + (float)rnd.NextDouble() * 20f;
                float tilt = 20f + (float)rnd.NextDouble() * 35f;
                var b = Part(root, PrimitiveType.Cube, "Blatt", blade, Vector3.zero, Vector3.zero, new Vector3(0.12f, 0.55f + (float)rnd.NextDouble() * 0.25f, 0.01f));
                var pivot = new GameObject("Blatt_Pivot").transform;
                pivot.SetParent(root.transform, false);
                pivot.localPosition = new Vector3(0, 0.6f, 0);
                pivot.localRotation = Quaternion.Euler(0, ang, tilt);
                b.transform.SetParent(pivot, false);
                b.transform.localPosition = new Vector3(0, b.transform.localScale.y * 0.5f, 0);
            }
            AddSway(root, 7f, 0.3f);
        }

        static void BuildSargassum(GameObject root)
        {
            var mat = Lit("Sargassum", new Color(0.36f, 0.31f, 0.12f), 0.3f);
            var rnd = new System.Random(2);
            for (int i = 0; i < 5; i++)
            {
                float h = 0.5f + (float)rnd.NextDouble() * 0.4f;
                var pivot = new GameObject("Ast").transform;
                pivot.SetParent(root.transform, false);
                pivot.localRotation = Quaternion.Euler(((float)rnd.NextDouble() - 0.5f) * 30f, 0, ((float)rnd.NextDouble() - 0.5f) * 30f);
                Part(pivot.gameObject, PrimitiveType.Cylinder, "Zweig", mat, new Vector3(0, h * 0.5f, 0), Vector3.zero, new Vector3(0.015f, h * 0.5f, 0.015f));
                for (int k = 0; k < 6; k++)
                    Part(pivot.gameObject, PrimitiveType.Sphere, "Blaeschen", mat,
                        new Vector3(((float)rnd.NextDouble() - 0.5f) * 0.12f, h * (0.3f + 0.7f * (float)rnd.NextDouble()), ((float)rnd.NextDouble() - 0.5f) * 0.12f),
                        Vector3.zero, Vector3.one * (0.03f + (float)rnd.NextDouble() * 0.03f));
            }
            AddSway(root, 8f, 0.4f);
        }

        static void BuildSeegras(GameObject root)
        {
            var mat = Lit("Seegras", new Color(0.24f, 0.42f, 0.17f), 0.3f);
            var rnd = new System.Random(3);
            for (int i = 0; i < 11; i++)
            {
                float h = 0.4f + (float)rnd.NextDouble() * 0.5f;
                var pivot = new GameObject("Halm").transform;
                pivot.SetParent(root.transform, false);
                pivot.localPosition = new Vector3(((float)rnd.NextDouble() - 0.5f) * 0.3f, 0, ((float)rnd.NextDouble() - 0.5f) * 0.3f);
                pivot.localRotation = Quaternion.Euler(((float)rnd.NextDouble() - 0.5f) * 25f, (float)rnd.NextDouble() * 180f, ((float)rnd.NextDouble() - 0.5f) * 25f);
                Part(pivot.gameObject, PrimitiveType.Cube, "Blatt", mat, new Vector3(0, h * 0.5f, 0), Vector3.zero, new Vector3(0.025f, h, 0.004f));
            }
            AddSway(root, 10f, 0.45f);
        }

        static void BuildRotalge(GameObject root)
        {
            var mat = Lit("Rotalge", new Color(0.45f, 0.14f, 0.24f), 0.25f);
            var rnd = new System.Random(4);
            for (int i = 0; i < 6; i++)
            {
                float s = 0.06f + (float)rnd.NextDouble() * 0.06f;
                Part(root, PrimitiveType.Sphere, "Bueschel", mat,
                    new Vector3(((float)rnd.NextDouble() - 0.5f) * 0.2f, s * 0.6f, ((float)rnd.NextDouble() - 0.5f) * 0.2f),
                    Vector3.zero, new Vector3(s, s * 1.4f, s));
            }
            AddSway(root, 4f, 0.3f);
        }

        static void BuildFlasche(GameObject root)
        {
            var glass = Lit("Muell_Glas", new Color(0.55f, 0.7f, 0.6f), 0.85f);
            Part(root, PrimitiveType.Cylinder, "Koerper", glass, Vector3.zero, new Vector3(0, 0, 90), new Vector3(0.07f, 0.1f, 0.07f));
            Part(root, PrimitiveType.Cylinder, "Hals", glass, new Vector3(0.14f, 0, 0), new Vector3(0, 0, 90), new Vector3(0.03f, 0.05f, 0.03f));
        }

        static void BuildDose(GameObject root)
        {
            var metal = Lit("Muell_Dose", new Color(0.55f, 0.2f, 0.16f), 0.6f, 0.7f);
            Part(root, PrimitiveType.Cylinder, "Dose", metal, Vector3.zero, new Vector3(0, 0, 90), new Vector3(0.065f, 0.06f, 0.065f));
        }

        static void BuildSeil(GameObject root)
        {
            var rope = Lit("Muell_Seil", new Color(0.42f, 0.36f, 0.25f), 0.15f);
            var rnd = new System.Random(5);
            Vector3 p = Vector3.zero;
            float dir = 0f;
            for (int i = 0; i < 14; i++)
            {
                dir += ((float)rnd.NextDouble() - 0.5f) * 70f;
                var q = Quaternion.Euler(0, dir, 0);
                Vector3 step = q * Vector3.forward * 0.12f;
                Part(root, PrimitiveType.Capsule, "Glied", rope, p + step * 0.5f, (q * Quaternion.Euler(90, 0, 0)).eulerAngles, new Vector3(0.03f, 0.07f, 0.03f));
                p += step;
            }
        }

        static void BuildNetz(GameObject root)
        {
            var net = Lit("Muell_Netz", new Color(0.2f, 0.3f, 0.25f), 0.2f);
            var rnd = new System.Random(6);
            for (int i = 0; i < 7; i++)
            {
                Part(root, PrimitiveType.Cube, "Strang", net,
                    new Vector3(((float)rnd.NextDouble() - 0.5f) * 0.6f, 0.01f, ((float)rnd.NextDouble() - 0.5f) * 0.4f),
                    new Vector3(0, (float)rnd.NextDouble() * 180f, 0), new Vector3(0.7f, 0.01f, 0.012f));
            }
        }

        static void BuildFischKlein(GameObject root)
        {
            var mat = Lit("Fisch_Silber", new Color(0.75f, 0.8f, 0.82f), 0.8f, 0.6f);
            Part(root, PrimitiveType.Sphere, "Koerper", mat, Vector3.zero, Vector3.zero, new Vector3(0.035f, 0.06f, 0.18f));
            Part(root, PrimitiveType.Cube, "Schwanz", mat, new Vector3(0, 0, -0.1f), new Vector3(0, 0, 0), new Vector3(0.005f, 0.05f, 0.03f));
            RemoveColliders(root);
        }

        static void BuildFischGross(GameObject root)
        {
            var mat = Lit("Fisch_Grau", new Color(0.42f, 0.47f, 0.5f), 0.6f, 0.3f);
            Part(root, PrimitiveType.Sphere, "Koerper", mat, Vector3.zero, Vector3.zero, new Vector3(0.12f, 0.2f, 0.5f));
            Part(root, PrimitiveType.Cube, "Schwanz", mat, new Vector3(0, 0, -0.28f), Vector3.zero, new Vector3(0.01f, 0.16f, 0.09f));
            Part(root, PrimitiveType.Cube, "Rueckenflosse", mat, new Vector3(0, 0.1f, 0f), Vector3.zero, new Vector3(0.008f, 0.07f, 0.12f));
            RemoveColliders(root);
        }

        // ------------------------------------------------------------------ Helfer

        static GameObject GetOrCreatePrefab(string name, System.Action<GameObject> build)
        {
            string path = $"{PrefabDir}/{name}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing; // nie ueberschreiben (Katjas Modelle!)

            var root = new GameObject(name);
            var placeholder = new GameObject("Platzhalter");
            placeholder.transform.SetParent(root.transform, false);
            build(placeholder);
            // PlantSway gehoert an die Wurzel, damit es auch mit dem echten Modell funktioniert
            var sway = placeholder.GetComponent<PlantSway>();
            if (sway != null)
            {
                var s = root.AddComponent<PlantSway>();
                s.amplitude = sway.amplitude;
                s.speed = sway.speed;
                Object.DestroyImmediate(sway);
            }
            RemoveColliders(root);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void AddSway(GameObject go, float amplitude, float speed)
        {
            var s = go.AddComponent<PlantSway>();
            s.amplitude = amplitude;
            s.speed = speed;
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

        static void RemoveColliders(GameObject root)
        {
            foreach (var c in root.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
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
