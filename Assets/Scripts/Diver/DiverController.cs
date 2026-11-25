using UnityEngine;

namespace ConcreteLife
{
    /// <summary>
    /// Die Taucherin schwimmt in einer Ebene vor der Mauer dorthin, wohin der Zeiger zeigt –
    /// mit Traegheit und etwas versetzt, damit sie den gezeigten Punkt nicht verdeckt.
    /// Modell-Konvention: Kopf zeigt entlang +Z, Ruecken entlang +Y, Pivot in der Koerpermitte.
    /// </summary>
    public class DiverController : MonoBehaviour
    {
        [Header("Schwimmebene")]
        [Tooltip("Z-Position der Ebene, in der sie schwimmt (Mauer-Vorderseite liegt bei ca. -0.3)")]
        public float swimPlaneZ = -1.8f;
        public Vector2 xRange = new Vector2(-8.5f, 8.5f);
        [Tooltip("Oben = knapp unter der Wasseroberflaeche (dort taucht sie auf)")]
        public Vector2 yRange = new Vector2(0.8f, 7.4f);

        [Header("Folgen")]
        [Tooltip("Versatz zum Zeigerpunkt (x wird zur Schwimmrichtung gespiegelt)")]
        public Vector2 followOffset = new Vector2(-0.9f, -0.6f);
        public float smoothTime = 0.55f;
        public float maxSpeed = 3.5f;

        [Header("Ausrichtung")]
        [Tooltip("Wie weit sie sich zur Kamera dreht (0 = reines Profil)")]
        public float turnToCamera = 25f;
        public float maxPitch = 35f;
        public float turnSpeed = 4f;

        [Header("Schweben")]
        public Transform visual;
        public float bobAmount = 0.05f;
        public float bobSpeed = 0.8f;

        public Vector3 Velocity => _velocity;

        Camera _cam;
        Vector3 _velocity;
        float _facing = 1f;
        Vector3 _target;

        void Start()
        {
            _cam = Camera.main;
            _target = transform.position;
            if (visual == null && transform.childCount > 0) visual = transform.GetChild(0);
        }

        void Update()
        {
            var input = PlayerInput.Instance;
            if (input != null && _cam != null)
            {
                Ray ray = _cam.ScreenPointToRay(input.PointerPosition);
                var plane = new Plane(Vector3.back, new Vector3(0f, 0f, swimPlaneZ));
                if (plane.Raycast(ray, out float d))
                {
                    Vector3 p = ray.GetPoint(d);
                    // Richtung bestimmt, auf welcher Seite des Zeigers sie bleibt
                    float dx = p.x - transform.position.x;
                    if (Mathf.Abs(dx) > 0.6f) _facing = Mathf.Sign(dx);
                    p.x += followOffset.x * _facing;
                    p.y += followOffset.y;
                    _target = new Vector3(Mathf.Clamp(p.x, xRange.x, xRange.y), Mathf.Clamp(p.y, yRange.x, yRange.y), swimPlaneZ);
                }
            }

            transform.position = Vector3.SmoothDamp(transform.position, _target, ref _velocity, smoothTime, maxSpeed);

            // Ausrichtung: in Schwimmrichtung, leicht zur Kamera gedreht, Nase folgt dem Auf/Ab
            if (Mathf.Abs(_velocity.x) > 0.15f) _facing = Mathf.Sign(_velocity.x);
            float yaw = _facing > 0 ? 90f + turnToCamera : -90f - turnToCamera;
            float speed = _velocity.magnitude;
            float pitch = Mathf.Clamp(-_velocity.y * 18f, -maxPitch, maxPitch) * Mathf.Clamp01(speed * 1.5f + 0.3f);
            float roll = Mathf.Sin(Time.time * 0.7f) * 3f;
            Quaternion targetRot = Quaternion.Euler(pitch, yaw, roll);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);

            if (visual != null)
                visual.localPosition = new Vector3(0f, Mathf.Sin(Time.time * bobSpeed * Mathf.PI * 2f) * bobAmount, 0f);
        }
    }
}
