using Bw.Entities;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Props.Network;
using Bw.UseCases.Props.View.Abstractions;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Props.View.Adapters
{
    public sealed class ThrowArcViewModel : IStateView<PropState>, IThrowArcViewModel
    {
        public IReadonlyProperty<bool> Aiming => _aiming;
        public IReadonlyProperty<ThrowArc> Arc => _arc;

        private readonly ViewableProperty<bool> _aiming = new(false);
        private readonly ViewableProperty<ThrowArc> _arc = new(default);

        private readonly ThrowLaunch _launch;
        private readonly ThrowTrajectory _trajectory;
        private readonly IControlledBy _controlledBy;
        private readonly IWeapon _prop;
        private readonly IWeaponsInHands _inHands;
        private readonly float _horizon;

        public ThrowArcViewModel(
            ThrowLaunch launch,
            ThrowTrajectory trajectory,
            IControlledBy controlledBy,
            IWeapon prop,
            IWeaponsInHands inHands,
            PropVfxConfig config)
        {
            _launch = launch;
            _trajectory = trajectory;
            _controlledBy = controlledBy;
            _prop = prop;
            _inHands = inHands;
            _horizon = config.ArcTime;
        }

        public void Show(PropState from, PropState to, float progress)
        {
            if (_controlledBy.Me.Value
                && _inHands.ByWeapon.TryGetValue(_prop, out var holder)
                && from.Phase == PropPhase.Holding
                && to.Phase == PropPhase.Holding)
            {
                var launched = _launch.Thrown(holder, Mathf.LerpAngle(from.Aim, to.Aim, progress));
                _arc.Value = new ThrowArc(_trajectory.CenterOf(launched), launched.Velocity, _trajectory.Gravity, _trajectory.ClearTime(launched, _horizon));
                _aiming.Value = true;
                return;
            }

            _aiming.Value = false;
        }
    }
}
