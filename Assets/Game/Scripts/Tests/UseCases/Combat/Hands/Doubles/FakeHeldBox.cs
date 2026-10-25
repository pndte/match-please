using System.Collections.Generic;
using Bw.Entities.Physics;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Hands.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Hands.Doubles
{
    public sealed class FakeHeldBox : IHeldBox
    {
        public BodyBox Box { get; }
        public int Layer { get; }
        public ISource<Unit> Turned => _turned;
        public IReadOnlyList<GripPose> Asked => _asked;

        private readonly Signal<Unit> _turned = new();
        private readonly List<GripPose> _asked = new();

        private Vector2 _offset;
        private float _angle;

        public FakeHeldBox(BodyBox box, int layer, Vector2 offset)
        {
            Box = box;
            Layer = layer;
            _offset = offset;
        }

        public GripPose PoseIn(HoldPose hands, GripPose previous)
        {
            _asked.Add(previous);
            return new GripPose(hands.Position + _offset, _angle, previous.Pushed + Vector2.up);
        }

        public void Turn(Vector2 offset, float angle)
        {
            _offset = offset;
            _angle = angle;
            _turned.Fire(Unit.Instance);
        }
    }
}
