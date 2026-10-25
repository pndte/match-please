using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Injection.Level;
using Bw.UseCases.Spawning;
using UnityEditor;
using UnityEngine;

namespace Bw.EditorTools.Level
{
    public static class MovingPlatformSpotGizmo
    {
        private static readonly Color Platform = new(1f, 0.75f, 0.2f, 0.9f);
        private static readonly Color Travel = new(1f, 0.75f, 0.2f, 0.4f);
        private static readonly Color Missing = new(1f, 0.2f, 0.2f, 0.9f);

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
        private static void Draw(MovingPlatformSpot spot, GizmoType type)
        {
            var at = (Vector2)spot.transform.position;
            if (spot.Platform == null || !spot.Platform.TryGetComponent<MovingPlatformInstaller>(out var installer))
            {
                Gizmos.color = Missing;
                Gizmos.DrawWireSphere(at, 0.5f);
                return;
            }

            var serialized = new SerializedObject(installer);
            var box = (BodyBox)serialized.FindProperty("_box").objectReferenceValue;
            var travel = serialized.FindProperty("_path.Travel").vector2Value;
            var size = box.WorldSize();
            var center = at + (Vector2)(box.transform.position - installer.transform.position) + box.WorldOffset();
            Gizmos.color = Platform;
            Gizmos.DrawWireCube(center, size);
            Gizmos.color = Travel;
            Gizmos.DrawWireCube(center + travel, size);
            Gizmos.DrawLine(center, center + travel);
        }
    }
}
