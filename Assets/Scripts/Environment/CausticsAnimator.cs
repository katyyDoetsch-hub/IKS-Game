using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ConcreteLife
{
    /// <summary>
    /// Spielt eine Folge von Caustics-Texturen als Light Cookie ab und laesst sie langsam wandern.
    /// Gehoert auf das Directional Light ("Sonne").
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Light))]
    public class CausticsAnimator : MonoBehaviour
    {
        public Texture2D[] frames;
        public float framesPerSecond = 10f;
        public Vector2 drift = new Vector2(0.12f, 0.04f);

        Light _light;
        UniversalAdditionalLightData _lightData;

        void OnEnable()
        {
            _light = GetComponent<Light>();
            _lightData = GetComponent<UniversalAdditionalLightData>();
        }

        void Update()
        {
            if (frames == null || frames.Length == 0) return;
            float t = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
            int i = (int)(t * framesPerSecond) % frames.Length;
            if (_light.cookie != frames[i]) _light.cookie = frames[i];
            if (_lightData != null) _lightData.lightCookieOffset = drift * t;
        }
    }
}
