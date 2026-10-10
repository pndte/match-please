using Bw.Entities;
using Bw.UseCases.Shooting.View.Crosshair.Abstractions;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Crosshair
{
    public sealed class WeaponSpreadCursor //TODO: бег, прыжок и полёт от отбрасывания курсор не сбивают
    {
        private readonly ISpread _spread;
        private readonly ISystemCursor _cursor;
        private readonly IChangableCamera _camera;
        private readonly Transform _weaponTransform;

        public WeaponSpreadCursor(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IReadonlyWeapon weapon,
            ISpread spread,
            ISystemCursor cursor,
            IChangableCamera camera,
            Transform weaponTransform)
        {
            _spread = spread;
            _cursor = cursor;
            _camera = camera;
            _weaponTransform = weaponTransform;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                weapon.Fired.Advise(controlledLifetime, Throw));
        }

        private void Throw(WeaponShot shot) //TODO: курсор бросает только клиент стрелка, сервер этого не проверяет: чит может его не бросать и стрелять без подброса
        {
            var pivot = (Vector2)_camera.Current.Value.WorldToScreenPoint(_weaponTransform.parent.position);
            var turn = Quaternion.Euler(0f, 0f, _spread.ThrowOf(shot.Aim, Random.Range(-1f, 1f)));
            _cursor.MoveTo(pivot + (Vector2)(turn * (_cursor.Position - pivot)));
        }
    }
}
