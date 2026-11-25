using UnityEngine;

namespace ConcreteLife
{
    /// <summary>
    /// Kamera faehrt der Taucherin sanft entlang der Mauer nach (begrenzt).
    /// Unter Wasser: Hoehe und Neigung haengen von ihrer Tiefe ab (oben mehr nach oben schauen).
    /// Auftauchen: schwimmt sie ganz nach oben, faehrt die Kamera bis zur Wasserlinie –
    /// das Bild ist dann geteilt (oben ueber Wasser, unten unter Wasser).
    /// Keine freie Kamerasteuerung.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;

        [Header("Seitlich")]
        [Tooltip("Wie weit die Kamera maximal zur Seite faehrt (m)")]
        public float xLimit = 4f;
        public float smoothTime = 0.9f;

        [Header("Tiefe der Taucherin")]
        public float diverYBottom = 0.8f;
        [Tooltip("Bis hier ist es normales Tauchen, darueber beginnt das Auftauchen")]
        public float diverYTopUnderwater = 6.2f;
        [Tooltip("Hoehe, in der sie an der Oberflaeche schwimmt")]
        public float diverYSurface = 7.4f;

        [Header("Neigung (negativ = nach oben)")]
        public float pitchAtBottom = -6f;
        public float pitchAtTop = -16f;
        public float pitchAtSurface = -2f;

        [Header("Kamerahoehe")]
        public float heightAtBottom = 1.0f;
        public float heightAtTop = 2.4f;

        [Header("Wasseroberflaeche")]
        public float waterLevel = 7.6f;
        [Tooltip("Wird ausgeblendet, wenn die Kamera genau auf der Wasserlinie ist (geteiltes Bild)")]
        public Renderer waterSurface;

        [Tooltip("Etwas langsamer als die Seitenfahrt, damit Neigen und Auftauchen ruhig bleiben")]
        public float tiltSmoothTime = 1.3f;

        /// 0 = unter Wasser, 1 = Kamera an der Wasserlinie
        public float SurfaceBlend { get; private set; }

        float _z, _velX, _velY, _velPitch, _pitch;

        void Start()
        {
            _z = transform.position.z;
            _pitch = transform.eulerAngles.x > 180f ? transform.eulerAngles.x - 360f : transform.eulerAngles.x;
            if (target == null)
            {
                var diver = FindFirstObjectByType<DiverController>();
                if (diver) target = diver.transform;
            }
        }

        static float Smooth01(float t) => t * t * (3f - 2f * t);

        void LateUpdate()
        {
            if (target == null) return;
            Vector3 t = target.position;
            float depth01 = Smooth01(Mathf.InverseLerp(diverYBottom, diverYTopUnderwater, t.y));
            float surf01 = Smooth01(Mathf.InverseLerp(diverYTopUnderwater, diverYSurface, t.y));
            SurfaceBlend = surf01;
            // an der Wasserlinie wuerden die Strahlen sich zu einem Kreuzmuster buendeln -> ausblenden
            GodRaySway.GlobalIntensity = 1f - 0.85f * surf01;

            float targetY = Mathf.Lerp(Mathf.Lerp(heightAtBottom, heightAtTop, depth01), waterLevel, surf01);
            float targetPitch = Mathf.Lerp(Mathf.Lerp(pitchAtBottom, pitchAtTop, depth01), pitchAtSurface, surf01);

            Vector3 p = transform.position;
            p.x = Mathf.SmoothDamp(p.x, Mathf.Clamp(t.x, -xLimit, xLimit), ref _velX, smoothTime);
            p.y = Mathf.SmoothDamp(p.y, targetY, ref _velY, tiltSmoothTime);
            p.z = _z;
            _pitch = Mathf.SmoothDamp(_pitch, targetPitch, ref _velPitch, tiltSmoothTime);
            transform.SetPositionAndRotation(p, Quaternion.Euler(_pitch, 0f, 0f));

            // Genau auf der Wasserlinie wuerde man die Oberflaeche nur als Strich sehen -> ausblenden.
            if (waterSurface != null)
                waterSurface.enabled = Mathf.Abs(p.y - waterLevel) > 0.12f;
        }
    }
}
