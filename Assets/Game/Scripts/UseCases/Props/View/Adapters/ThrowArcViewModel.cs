using Bw.Entities;
using Bw.Entities.Simulation;
using Bw.UseCases.Props.Network.Prediction;
using Bw.UseCases.Props.View.Abstractions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
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
        private readonly IWeaponHold _hold;
        private readonly float _horizon;

        public ThrowArcViewModel(
            ThrowLaunch launch,
            ThrowTrajectory trajectory,
            IControlledBy controlledBy,
            IWeaponHold hold,
            PropVfxConfig config)
        {
            _launch = launch;
            _trajectory = trajectory;
            _controlledBy = controlledBy;
            _hold = hold;
            _horizon = config.ArcTime;
        }

        public void Show(PropState from, PropState to, float progress)
        {
            var aiming = _controlledBy.Me.Value
                && _hold.HeldLifetime.Value.IsAlive
                && from.Phase == PropPhase.Holding
                && to.Phase == PropPhase.Holding;
            if (aiming)
            {
                var launched = _launch.FromHands(Mathf.LerpAngle(from.Aim, to.Aim, progress));
                _arc.Value = new ThrowArc(_trajectory.CenterOf(launched), launched.Velocity, _trajectory.Gravity, _trajectory.ClearTime(launched, _horizon));
            }

            _aiming.Value = aiming;
        }
    }
}
