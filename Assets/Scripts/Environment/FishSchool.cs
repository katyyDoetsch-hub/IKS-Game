using System.Collections.Generic;
using UnityEngine;

namespace ConcreteLife
{
    /// <summary>
    /// Ein gemischter Fischschwarm (viele kleine + wenige grosse Fische), der in unregelmaessigen
    /// Abstaenden an der Mauer vorbeizieht und nicht bleibt.
    /// Die Fisch-Prefabs sind Platzhalter und koennen durch echte Modelle ersetzt werden
    /// (Modell schaut entlang +Z).
    /// </summary>
    public class FishSchool : MonoBehaviour
    {
        public GameObject smallFishPrefab;
        public GameObject bigFishPrefab;
        public int smallCount = 26;
        public int bigCount = 2;

        [Header("Timing (unregelmaessig)")]
        public Vector2 shortPause = new Vector2(4f, 10f);
        public Vector2 longPause = new Vector2(18f, 60f);
        [Range(0f, 1f)] public float shortPauseChance = 0.3f;
        public float firstDelay = 3f;

        [Header("Bahn")]
        public float speed = 1.6f;
        public float travelHalfWidth = 20f;
        public Vector2 heightRange = new Vector2(2f, 5.5f);
        public Vector2 depthRange = new Vector2(-3.5f, -1.2f);
        public Vector3 schoolSize = new Vector3(1.4f, 0.6f, 0.9f);

        class Fish
        {
            public Transform t;
            public Vector3 offset;
            public float phase;
            public float lag;
            public Vector3 lastPos;
        }

        readonly List<Fish> _fish = new List<Fish>();
        float _nextTime, _progress, _duration;
        bool _active;
        Vector3 _start, _end, _dir;

        void Start()
        {
            for (int i = 0; i < smallCount; i++) Spawn(smallFishPrefab, false);
            for (int i = 0; i < bigCount; i++) Spawn(bigFishPrefab, true);
            _nextTime = Time.time + firstDelay;
        }

        void Spawn(GameObject prefab, bool big)
        {
            if (prefab == null) return;
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            // kleine Groessenunterschiede, damit kein Fisch dem anderen gleicht
            go.transform.localScale *= Random.Range(0.8f, 1.2f);
            _fish.Add(new Fish
            {
                t = go.transform,
                phase = Random.value * 100f,
                lag = big ? Random.Range(1.2f, 2.5f) : Random.Range(-0.3f, 0.3f),
                offset = big
                    ? new Vector3(0f, Random.Range(-0.6f, 0.6f), Random.Range(-0.6f, 0.6f))
                    : Vector3.Scale(Random.insideUnitSphere, schoolSize),
            });
        }

        void Launch()
        {
            float side = Random.value < 0.5f ? 1f : -1f;
            float y = Random.Range(heightRange.x, heightRange.y);
            float z = Random.Range(depthRange.x, depthRange.y);
            _start = new Vector3(-side * travelHalfWidth, y, z);
            _end = new Vector3(side * travelHalfWidth, y + Random.Range(-1.5f, 1.5f), z + Random.Range(-0.8f, 0.8f));
            _dir = (_end - _start).normalized;
            _duration = Vector3.Distance(_start, _end) / (speed * Random.Range(0.8f, 1.3f));
            _progress = 0f;
            foreach (var f in _fish)
            {
                f.t.gameObject.SetActive(true);
                f.lastPos = _start + f.offset - _dir * f.lag;
                f.t.position = f.lastPos;
            }
            _active = true;
        }

        void Update()
        {
            if (!_active)
            {
                if (Time.time >= _nextTime) Launch();
                return;
            }

            _progress += Time.deltaTime / _duration;
            float wave = Mathf.Sin(_progress * Mathf.PI * 2f) * 0.4f;
            Vector3 center = Vector3.Lerp(_start, _end, _progress) + Vector3.up * wave;

            foreach (var f in _fish)
            {
                float t = Time.time * 0.6f + f.phase;
                var jitter = new Vector3(Mathf.PerlinNoise(t, f.phase) - 0.5f, Mathf.PerlinNoise(f.phase, t) - 0.5f, Mathf.PerlinNoise(t, t * 0.7f) - 0.5f) * 0.5f;
                Vector3 pos = center + f.offset + jitter - _dir * f.lag;
                Vector3 vel = pos - f.lastPos;
                if (vel.sqrMagnitude > 1e-6f)
                    f.t.rotation = Quaternion.Slerp(f.t.rotation, Quaternion.LookRotation(vel.normalized, Vector3.up), Time.deltaTime * 6f);
                f.t.position = pos;
                f.lastPos = pos;
            }

            if (_progress >= 1.15f)
            {
                foreach (var f in _fish) f.t.gameObject.SetActive(false);
                _active = false;
                bool shortOne = Random.value < shortPauseChance;
                Vector2 r = shortOne ? shortPause : longPause;
                _nextTime = Time.time + Random.Range(r.x, r.y);
            }
        }
    }
}
