using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics.Extensions
{
    public static class BoxCollider2DExtensions
    {
        public static PolygonGeometry WorldGeometry(this BoxCollider2D box)
        {
            var transform = box.transform;
            var center = (Vector2)transform.TransformPoint(box.offset);
            var rotation = new PhysicsRotate(transform.eulerAngles.z * Mathf.Deg2Rad);
            return PolygonGeometry.CreateBox(box.WorldSize(), box.edgeRadius, new PhysicsTransform(center, rotation), false);
        }

        public static PolygonGeometry LocalGeometry(this BoxCollider2D box) =>
            PolygonGeometry.CreateBox(box.WorldSize(), box.edgeRadius, new PhysicsTransform(box.WorldOffset()), false);

        public static Vector2 WorldSize(this BoxCollider2D box)
        {
            var scale = box.transform.lossyScale;
            return new Vector2(box.size.x * Mathf.Abs(scale.x), box.size.y * Mathf.Abs(scale.y));
        }

        public static Vector2 WorldOffset(this BoxCollider2D box) =>
            Vector2.Scale(box.offset, box.transform.lossyScale);

        public static Rect WorldRect(this BoxCollider2D box)
        {
            var size = box.WorldSize();
            return new Rect((Vector2)box.transform.position + box.WorldOffset() - size / 2f, size);
        }

        public static PhysicsMask Categories(this BoxCollider2D box)
        {
            var categories = PhysicsMask.None;
            categories.SetBit(box.gameObject.layer);
            if (box.IsOneWay())
                categories.SetBit(PhysicsCategories.OneWay);

            return categories;
        }

        public static bool IsOneWay(this BoxCollider2D box) =>
            box.usedByEffector && box.TryGetComponent(out PlatformEffector2D effector) && effector.enabled && effector.useOneWay;
    }
}
