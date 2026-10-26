using UnityEngine;

namespace Bw.UseCases.Character.View.Rig
{
    public static class RigLegPlacer
    {
        public static void Place(RigFoot foot, Transform root, CharacterRigLook look)
        {
            Vector2 ankle = root.InverseTransformPoint(foot.Ankle.position);
            Vector2 hip = root.InverseTransformPoint(foot.Hip.position);
            var span = hip - ankle;
            var length = Mathf.Max(span.magnitude, look.LegInset * 2f);
            var direction = span / length;
            var rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg);
            var leg = foot.Leg.transform;
            leg.SetLocalPositionAndRotation(ankle + direction * look.LegInset, rotation);
            leg.localScale = new Vector3(1f, (length - look.LegInset) / look.LegLength, 1f);
            foot.Tick.transform.SetLocalPositionAndRotation(hip - direction * look.TickDrop, rotation);
        }
    }
}
