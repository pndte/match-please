using Bw.Entities.Physics;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Hands.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using UnityEngine;

namespace Bw.UseCases.Props
{
    public sealed class HeldPropPose : IStateView<PropState>, IHeldBox
    {
        public BodyBox Box { get; }
        public int Layer { get; }
        public ISource<Unit> Turned => _turned;

        private readonly HeldPropPlacer _placer;
        private readonly PropSwing _swing;
        private readonly Signal<Unit> _turned = new();

        private float _angle;

        public HeldPropPose(
            BodyBox shape,
            int layer,
            IReadonlySimulation<PropState> simulation,
            HeldPropPlacer placer,
            PropSwing swing)
        {
            Box = shape;
            Layer = layer;
            _placer = placer;
            _swing = swing;
            _angle = simulation.State.Value.Aim;
        }

        public void Show(PropState from, PropState to, float progress)
        {
            _angle = Mathf.LerpAngle(from.Aim, to.Aim, progress) + Mathf.Lerp(_swing.AngleOf(from), _swing.AngleOf(to), progress);
            _turned.Fire(Unit.Instance);
        }

        public GripPose PoseIn(HoldPose hands, GripPose previous) =>
            _placer.PoseIn(hands.Hold, _angle, previous.Pushed);

        public GripPose PoseAt(HoldPose hands, float angle) =>
            _placer.PoseIn(hands.Hold, angle, Vector2.zero);
    }
}
