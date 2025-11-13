#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Sonity;

namespace DefaultNamespace
{
    public class RandomEmitter : MonoBehaviour
    {
        [SerializeField] private SoundEvent soundEvent;

        [SerializeField] private Vector2 intervalRange = new Vector2(3f, 8f);

        [SerializeField] private float innerRadius = 2f;
        [SerializeField] private float outerRadius = 8f;

        [SerializeField] private bool spawnAroundEmitter = true;

        private float _timer;

        private void Start()
        {
            ResetTimer();
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                PlayRandomSound();
                ResetTimer();
            }
        }

        private void PlayRandomSound()
        {
            if (soundEvent == null)
                return;

            Vector3 position = transform.position;

            if (spawnAroundEmitter)
            {
                Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(innerRadius, outerRadius);
                position += new Vector3(randomCircle.x, 0f, randomCircle.y);
            }

            soundEvent.PlayAtPosition(transform, position);
        }

        private void ResetTimer()
        {
            _timer = Random.Range(intervalRange.x, intervalRange.y);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Color outerColor = new Color(0.3f, 0.7f, 1f, 0.6f);
            Color innerColor = new Color(0.3f, 0.7f, 1f, 0.9f);
            
            DrawCircle(transform.position, outerRadius, outerColor);
            DrawCircle(transform.position, innerRadius, innerColor);
        }

        private void DrawCircle(Vector3 center, float radius, Color color, int segments = 64)
        {
            Gizmos.color = color;
            Vector3 prevPoint = center + new Vector3(Mathf.Cos(0f), 0f, Mathf.Sin(0f)) * radius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }
#endif
    }
}
