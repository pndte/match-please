using UnityEngine;

namespace Bw.Entities.Physics
{
    public sealed class BodyBox : MonoBehaviour
    {
        public Vector2 Size => _size;
        public Vector2 Offset => _offset;
        public bool IsOneWay => _oneWay;

        [SerializeField] private Vector2 _size = Vector2.one;
        [SerializeField] private Vector2 _offset;
        [SerializeField] private bool _oneWay;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _oneWay ? new Color(0.45f, 1f, 0.45f) : new Color(0.35f, 0.85f, 1f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(_offset, _size);
        }
    }
}
