using UnityEngine;

namespace ConcreteLife
{
    /// <summary>
    /// Laesst einen Lichtstrahl langsam in der Helligkeit pulsieren und leicht schwanken.
    /// </summary>
    [ExecuteAlways]
    public class GodRaySway : MonoBehaviour
    {
        /// Gemeinsame Helligkeit aller Strahlen (z. B. beim Auftauchen abgesenkt).
        public static float GlobalIntensity = 1f;

        [ColorUsage(false, true)] public Color baseColor = new Color(0.1f, 0.15f, 0.13f);
        public float speed = 0.15f;
        [Range(0f, 1f)] public float intensityVariation = 0.6f;
        public float swayAngle = 2.5f;

        Renderer _renderer;
        MaterialPropertyBlock _mpb;
        Quaternion _baseRotation;
        float _seed;

        void OnEnable()
        {
            _renderer = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();
            _baseRotation = transform.localRotation;
            _seed = transform.GetSiblingIndex() * 7.31f + 3.1f;
        }

        void Update()
        {
            if (_renderer == null) return;
            float t = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
            float n = Mathf.PerlinNoise(_seed, t * speed);
            float k = 1f - intensityVariation + intensityVariation * 2f * n;
            Color c = baseColor * (k * (Application.isPlaying ? GlobalIntensity : 1f));
            c.a = 1f; // nur die Helligkeit pulsiert, nicht die Deckkraft
            _mpb.SetColor("_Color", c);
            _renderer.SetPropertyBlock(_mpb);

            if (Application.isPlaying)
            {
                float a = (Mathf.PerlinNoise(_seed + 11f, t * speed * 0.5f) - 0.5f) * 2f * swayAngle;
                transform.localRotation = _baseRotation * Quaternion.Euler(0f, 0f, a);
            }
        }
    }
}
