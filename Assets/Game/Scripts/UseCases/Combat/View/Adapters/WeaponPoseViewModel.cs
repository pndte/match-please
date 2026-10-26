using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Hands.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Combat.View.Adapters
{
    public sealed class WeaponPoseViewModel<TState> : IStateView<TState>, IWeaponPoseViewModel where TState : struct, IWeaponState
    {
        public Vector2 Pivot => _views.ByCharacter[_inHands.HolderOf(_weapon)].Position;
        public float Aim { get; private set; }
        public float Radius => _orbit.RadiusAt(Pivot, Aim);
        public float RadiusSpeed => _config.RadiusChangeSpeed;

        private readonly IWeapon _weapon;
        private readonly IReadonlyWeaponsInHands _inHands;
        private readonly WeaponOrbit _orbit;
        private readonly WeaponRotationConfig _config;
        private readonly ICharacterViews _views;

        public WeaponPoseViewModel(
            IWeapon weapon,
            IReadonlyWeaponsInHands inHands,
            WeaponOrbit orbit,
            WeaponRotationConfig config,
            ICharacterViews views)
        {
            _weapon = weapon;
            _inHands = inHands;
            _orbit = orbit;
            _config = config;
            _views = views;
        }

        public void Show(TState from, TState to, float progress) =>
            Aim = Mathf.LerpAngle(from.Aim, to.Aim, progress);
    }
}
