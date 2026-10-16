using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics.Extensions
{
    public static class BodyBoxExtensions
    {
        public static PolygonGeometry WorldGeometry(this BodyBox box)
        {
            var transform = box.transform;
            var center = (Vector2)transform.TransformPoint(box.Offset);
            var rotation = new PhysicsRotate(transform.eulerAngles.z * Mathf.Deg2Rad);
            return PolygonGeometry.CreateBox(box.WorldSize(), 0f, new PhysicsTransform(center, rotation), false);
        }

        public static PolygonGeometry LocalGeometry(this BodyBox box) =>
            PolygonGeometry.CreateBox(box.WorldSize(), 0f, new PhysicsTransform(box.WorldOffset()), false);

        public static Vector2 WorldSize(this BodyBox box)
        {
            var scale = box.transform.lossyScale;
            return new Vector2(box.Size.x * Mathf.Abs(scale.x), box.Size.y * Mathf.Abs(scale.y));
        }

        public static Vector2 WorldOffset(this BodyBox box) =>
            Vector2.Scale(box.Offset, box.transform.lossyScale);

        public static Rect WorldRect(this BodyBox box)
        {
            var size = box.WorldSize();
            return new Rect((Vector2)box.transform.position + box.WorldOffset() - size / 2f, size);
        }

        public static PhysicsMask Categories(this BodyBox box)
        {
            var categories = PhysicsMask.None;
            categories.SetBit(box.gameObject.layer);
            if (box.IsOneWay)
                categories.SetBit(PhysicsCategories.OneWay);

            return categories;
        }
    }
}
