using System.Collections.Generic;
using UnityEngine;

namespace Bw.UseCases.Character.View.Grips
{
    public sealed class HandGrip : MonoBehaviour
    {
        public Sprite Hand => _hand;
        public bool Behind => _behind;

        [SerializeField] private Sprite _hand;
        [SerializeField] private bool _behind;

#if UNITY_EDITOR
        private static readonly Dictionary<Sprite, Mesh> GizmoMeshes = new();
        private static Material _gizmoMaterial;

        private void OnDrawGizmos()
        {
            if (Application.isPlaying || _hand == null)
                return;

            if (_gizmoMaterial == null)
                _gizmoMaterial = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.HideAndDontSave };

            _gizmoMaterial.mainTexture = _hand.texture;
            _gizmoMaterial.color = _behind ? new Color(0.75f, 0.75f, 0.75f, 0.8f) : Color.white;
            _gizmoMaterial.SetPass(0);
            Graphics.DrawMeshNow(GizmoMeshOf(_hand), transform.localToWorldMatrix);
            Gizmos.color = _behind ? new Color(0.4f, 0.6f, 1f) : new Color(1f, 0.55f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, 0.04f);
        }

        private static Mesh GizmoMeshOf(Sprite sprite)
        {
            if (GizmoMeshes.TryGetValue(sprite, out var mesh) && mesh != null)
                return mesh;

            mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
            mesh.SetVertices(System.Array.ConvertAll(sprite.vertices, vertex => (Vector3)vertex));
            mesh.SetUVs(0, sprite.uv);
            mesh.SetTriangles(System.Array.ConvertAll(sprite.triangles, index => (int)index), 0);
            GizmoMeshes[sprite] = mesh;
            return mesh;
        }
#endif
    }
}
