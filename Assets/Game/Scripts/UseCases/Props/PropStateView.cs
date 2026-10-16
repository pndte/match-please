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
        private readonly BoxCollider2D _shape;
        private readonly Rigidbody2D _rigidbody;
        private readonly HeldPropConfig _config;
        private readonly HeldPropGrip _grip;
        private readonly Signal<Unit> _turned = new();

        private float _aim;

        public PropStateView(
            BoxCollider2D shape,
            Rigidbody2D rigidbody,
            IReadonlySimulation<PropState> simulation,
            HeldPropConfig config,
            HeldPropGrip grip)
        {
            _prop = shape.transform;
            _shape = shape;
            _rigidbody = rigidbody;
            _config = config;
            _grip = grip;
            _aim = simulation.State.Value.Aim;
        }

        public void Show(PropState from, PropState to, float progress)
        {
            _aim = Mathf.LerpAngle(from.Aim, to.Aim, progress);
            _turned.Fire(Unit.Instance);
        }

        public void PlaceIn(Rect holder) //TODO: поза в руках не знает стен и пола — у стены и прицелом вниз на земле ящик рисуется в них (отпущенный, он встаёт на ближайшее свободное место вокруг держателя); рук у держателя нет, ящик висит перед ним, как парящее оружие
        {
            var scale = (Vector2)_prop.lossyScale;
            var center = holder.center + _config.HeldOffset(_aim, holder.size / 2f, Vector2.Scale(_shape.size, scale) / 2f);
            var position = center - Vector2.Scale(_shape.offset, scale);
            _prop.SetPositionAndRotation(new Vector3(position.x, position.y, _prop.position.z), Quaternion.identity);
            _rigidbody.position = position;
            _grip.Place(center);
        }
    }
}
