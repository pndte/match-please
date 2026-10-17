using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
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
        private readonly Signal<Unit> _turned = new();

        private float _aim;

        public PropStateView(
            BodyBox shape,
            IReadonlySimulation<PropState> simulation,
            HeldPropPlacement placement,
            HeldPropGrip grip)
        {
            _prop = shape.transform;
            _shape = shape;
            _placement = placement;
            _grip = grip;
            _aim = simulation.State.Value.Aim;
        }

        public void Show(PropState from, PropState to, float progress)
        {
            _aim = Mathf.LerpAngle(from.Aim, to.Aim, progress);
            _turned.Fire(Unit.Instance);
        }

        public void PlaceIn(Rect oval) //TODO: рук у держателя нет, ящик висит перед ним, как парящее оружие
        {
            _placement.Place(oval, _aim);
            var center = _placement.Center;
            var turn = Quaternion.Euler(0f, 0f, _aim);
            var position = center - (Vector2)(turn * _shape.WorldOffset());
            _prop.SetPositionAndRotation(new Vector3(position.x, position.y, _prop.position.z), turn);
            _grip.Place(center, _aim);
        }
    }
}
