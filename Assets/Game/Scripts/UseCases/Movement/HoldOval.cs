using UnityEngine;

namespace Bw.UseCases.Movement
{
    public sealed class HoldOval : MonoBehaviour
    {
        private const int GizmoSegments = 48;

        public Vector2 Center => _center;
        public Vector2 Radii => _radii;

        [SerializeField] private Vector2 _center;
        [SerializeField] private Vector2 _radii = new(0.5f, 1f);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.7f, 0.25f);
            Gizmos.matrix = transform.localToWorldMatrix;
            var previous = PointAt(0f);
            for (var segment = 1; segment <= GizmoSegments; segment++)
            {
                var next = PointAt(segment * Mathf.PI * 2f / GizmoSegments);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }

        private Vector3 PointAt(float angle) =>
            _center + Vector2.Scale(_radii, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
    }
}
