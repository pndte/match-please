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

        private readonly HeldPropPlacement _placement;
        private readonly PropSwing _swing;
        private readonly Signal<Unit> _turned = new();

        private float _angle;

        public HeldPropPose(
            BodyBox shape,
            int layer,
            IReadonlySimulation<PropState> simulation,
            HeldPropPlacement placement,
            PropSwing swing)
        {
            Box = shape;
            Layer = layer;
            _placement = placement;
            _swing = swing;
            _angle = simulation.State.Value.Aim;
        }

        public void Show(PropState from, PropState to, float progress) //TODO: поправка прицела ящика после отката не сглаживается, как у оружия (SmoothedAimView): угол хвата здесь — и картинка, и тело хвата в мире, сдвиг надо отделить от тела
        {
            _angle = Mathf.LerpAngle(from.Aim, to.Aim, progress) + Mathf.Lerp(_swing.AngleOf(from), _swing.AngleOf(to), progress);
            _turned.Fire(Unit.Instance);
        }

        public GripPose PoseIn(HoldPose hands, GripPose previous) =>
            _placement.PoseIn(hands.Hold, _angle, previous.Pushed);

        public GripPose PoseAt(HoldPose hands, float angle) =>
            _placement.PoseIn(hands.Hold, angle, Vector2.zero);
    }
}
