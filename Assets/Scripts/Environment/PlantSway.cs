using UnityEngine;

namespace ConcreteLife
{
    /// <summary>
    /// Laesst eine Pflanze sanft in der Stroemung schwanken.
    /// Jede Pflanze schwankt anders (Phase aus der Position), damit nichts synchron wirkt.
    /// Pivot der Pflanze = Fusspunkt am Boden.
    /// </summary>
    public class PlantSway : MonoBehaviour
    {
        public float amplitude = 6f;
        public float speed = 0.35f;

        Quaternion _baseRotation;
        float _seed;

        void Start()
        {
            _baseRotation = transform.localRotation;
            Vector3 p = transform.position;
            _seed = p.x * 1.37f + p.z * 2.11f + 50f;
        }

        void Update()
        {
            float t = Time.time * speed;
            float ax = (Mathf.PerlinNoise(_seed, t) - 0.5f) * 2f * amplitude;
            float az = (Mathf.PerlinNoise(_seed + 5.3f, t * 0.8f) - 0.5f) * 2f * amplitude;
            transform.localRotation = _baseRotation * Quaternion.Euler(ax, 0f, az);
        }
    }
}
