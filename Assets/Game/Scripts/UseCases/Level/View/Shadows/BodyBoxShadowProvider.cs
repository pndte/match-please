using System;
using Bw.Entities.Physics;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Bw.UseCases.Level.View.Shadows
{
    [Serializable]
    public sealed class BodyBoxShadowProvider : ShadowShape2DProvider
    {
        private static readonly int[] Edges = { 0, 1, 1, 2, 2, 3, 3, 0 };

        private Vector2 _size;
        private Vector2 _offset;
        private bool _shaped;

        public override string ProviderName(string componentName) =>
            "Body Box";

        public override bool IsShapeSource(Component sourceComponent) =>
            sourceComponent is BodyBox;

        public override void Enabled(Component sourceComponent, ShadowShape2D persistantShadowShape) =>
            _shaped = false;

        public override void OnPersistantDataCreated(Component sourceComponent, ShadowShape2D persistantShadowShape) =>
            Reshape((BodyBox)sourceComponent, persistantShadowShape);

        public override void OnBeforeRender(Component sourceComponent, Bounds worldCullingBounds, ShadowShape2D persistantShadowShape)
        {
            var box = (BodyBox)sourceComponent;
            if (!_shaped || box.Size != _size || box.Offset != _offset)
                Reshape(box, persistantShadowShape);
        }

        private void Reshape(BodyBox box, ShadowShape2D shadow)
        {
            var min = box.Offset - box.Size / 2f;
            var max = box.Offset + box.Size / 2f;
            using var corners = new NativeArray<Vector3>(new Vector3[] { min, new(max.x, min.y), max, new(min.x, max.y) }, Allocator.Temp);
            using var edges = new NativeArray<int>(Edges, Allocator.Temp);
            using var radii = new NativeArray<float>(corners.Length, Allocator.Temp);
            shadow.SetShape(corners, edges, radii, Matrix4x4.identity, ShadowShape2D.WindingOrder.CounterClockwise, true, !box.TryGetComponent<Renderer>(out _));
            _size = box.Size;
            _offset = box.Offset;
            _shaped = true;
        }
    }
}
