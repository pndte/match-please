using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Props.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using UnityEngine;

namespace Bw.UseCases.Props
{
    public sealed class PropStateView : IStateView<PropState>, IHeldItem
    {
        public ISource<Unit> Turned => _turned;

        private readonly Transform _prop;
        private readonly BodyBox _shape;
        private readonly HeldPropPlacement _placement;
        private readonly HeldPropGrip _grip;
        private readonly HeldPropConfig _config;
        private readonly ISimulationStep _step;
        private readonly Signal<Unit> _turned = new();

        private float _angle;

        public PropStateView(
            BodyBox shape,
            IReadonlySimulation<PropState> simulation,
            HeldPropPlacement placement,
            HeldPropGrip grip,
            HeldPropConfig config,
            ISimulationStep step)
        {
            _prop = shape.transform;
            _shape = shape;
            _placement = placement;
            _grip = grip;
            _config = config;
            _step = step;
            _angle = simulation.State.Value.Aim;
        }

        public void Show(PropState from, PropState to, float progress)
        {
            var aim = Mathf.LerpAngle(from.Aim, to.Aim, progress);
            _angle = aim + Mathf.Lerp(SwingOf(from), SwingOf(to), progress);
            _turned.Fire(Unit.Instance);
        }

        public void PlaceIn(Rect oval)
        {
            _placement.Place(oval, _angle);
            var center = _placement.Center;
            var turn = Quaternion.Euler(0f, 0f, _angle);
            var position = center - (Vector2)(turn * _shape.WorldOffset());
            _prop.SetPositionAndRotation(new Vector3(position.x, position.y, _prop.position.z), turn);
            _grip.Place(center, _angle);
        }

        private float SwingOf(PropState state)
        {
            if (state.Phase != PropPhase.WindingUp)
                return 0f;

            var progress = Mathf.Clamp01(state.PhaseTicks / (float)_config.WindUpTicks(_step));
            var side = Mathf.Sign(Mathf.Cos(state.Aim * Mathf.Deg2Rad));
            return side * _config.WindUpAngle * _config.WindUpSwing.Evaluate(progress);
        }
    }
}
