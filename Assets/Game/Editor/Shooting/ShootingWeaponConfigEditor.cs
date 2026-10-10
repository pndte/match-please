using System;
using System.Collections.Generic;
using Bw.UseCases.Shooting.View.Recoil;
using Bw.UseCases.Shooting.Weapon;
using UnityEditor;

namespace Bw.EditorTools.Shooting
{
    [CustomEditor(typeof(ShootingWeaponConfig))]
    public sealed class ShootingWeaponConfigEditor : WeaponFeelEditor
    {
        protected override PreviewWeapon PreviewedWeapon() =>
            PreviewWeapon.ForShooting((ShootingWeaponConfig)target);

        protected override IReadOnlyList<LinkedConfigSection> LinkedSections(PreviewWeapon weapon) =>
            AssetDatabase.Contains(weapon.Recoil)
                ? new[] { new LinkedConfigSection(weapon.Recoil, "Recoil", nameof(WeaponRecoilConfig.Kick), nameof(WeaponRecoilConfig.Tilt)) }
                : Array.Empty<LinkedConfigSection>();
    }
}
