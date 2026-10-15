using Bw.Entities;
using Bw.UseCases.Shooting.Weapon.Pickup.Abstractions;
using Bw.UseCases.Shooting.Weapon.Pickup.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Pickup
{
    public sealed class PickupTargeting : IPickupTargeting
    {
        public IReadonlyProperty<PickupTarget> Target => _target;

        private readonly ViewableProperty<PickupTarget> _target = new(PickupTarget.Nothing);
        private readonly IReadonlyFloorWeapons _weapons;
        private readonly IChangableCamera _camera;
        private readonly WeaponPickupConfig _config;
        private readonly Transform _body;

        public PickupTargeting(
            Lifetime lifetime,
            IControlledBy controlledBy,
            IReadonlyFloorWeapons weapons,
            IChangableCamera camera,
            WeaponPickupConfig config,
            Transform body)
        {
            _weapons = weapons;
            _camera = camera;
            _config = config;
            _body = body;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
            {
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(_ => Retarget());
                controlledLifetime.OnTermination(() => _target.Value = PickupTarget.Nothing);
            });
        }

        private void Retarget()
        {
            if (!_weapons.TryFindPickable(CursorPoint(), _body.position, _config, out var weapon))
            {
                _target.Value = PickupTarget.Nothing;
                return;
            }

            if (!_target.Value.IsWeapon(weapon))
                _target.Value = PickupTarget.Of(weapon);
        }

        private Vector2 CursorPoint()
        {
            var camera = _camera.Current.Value;
            var mouse = Input.mousePosition; //TODO: new input system
            mouse.z = camera.nearClipPlane;
            return camera.ScreenToWorldPoint(mouse);
        }
    }
}
