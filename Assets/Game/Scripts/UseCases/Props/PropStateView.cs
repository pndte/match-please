using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Physics.Abstractions;
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
        private readonly IMovementBody _body;
        private readonly HeldPropConfig _config;
        private readonly Signal<Unit> _turned = new();

        private float _aim;

        public PropStateView(
            Transform prop,
            BoxCollider2D shape,
            IMovementBody body,
            IReadonlySimulation<PropState> simulation,
            HeldPropConfig config)
        {
            _prop = prop;
            _shape = shape;
            _body = body;
            _config = config;
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
            _prop.rotation = Quaternion.identity;
            _body.Place(center - Vector2.Scale(_shape.offset, scale));
        }
    }
}
