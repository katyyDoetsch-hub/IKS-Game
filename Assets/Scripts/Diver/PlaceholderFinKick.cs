using UnityEngine;

namespace ConcreteLife
{
    /// <summary>
    /// Nur fuer den Platzhalter: laesst die Flossen schlagen, schneller wenn sie schwimmt.
    /// Faellt mit dem Platzhalter weg, wenn das echte (geriggte) Modell eingesetzt wird.
    /// </summary>
    public class PlaceholderFinKick : MonoBehaviour
    {
        public Transform finLeft;
        public Transform finRight;
        public float idleSpeed = 1.2f;
        public float swimSpeed = 4f;
        public float amplitude = 22f;

        DiverController _diver;
        float _phase;

        void Start()
        {
            _diver = GetComponentInParent<DiverController>();
        }

        void Update()
        {
            float v = _diver != null ? Mathf.Clamp01(_diver.Velocity.magnitude / 2f) : 0f;
            _phase += Time.deltaTime * Mathf.Lerp(idleSpeed, swimSpeed, v);
            float a = Mathf.Sin(_phase * Mathf.PI * 2f) * amplitude * Mathf.Lerp(0.4f, 1f, v);
            if (finLeft) finLeft.localRotation = Quaternion.Euler(a, 0f, 0f);
            if (finRight) finRight.localRotation = Quaternion.Euler(-a, 0f, 0f);
        }
    }
}
