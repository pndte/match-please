using System;
using System.Collections.Generic;
using Bw.UseCases.Shooting.View.Recoil;
using Bw.UseCases.Shooting.Weapon;
using UnityEditor;

namespace Bw.EditorTools.Shooting
{
    [CustomEditor(typeof(WeaponRecoilConfig))]
    public sealed class WeaponRecoilConfigEditor : WeaponFeelEditor
    {
        protected override PreviewWeapon PreviewedWeapon() =>
            PreviewWeapon.ForRecoil((WeaponRecoilConfig)target);

        protected override IReadOnlyList<LinkedConfigSection> LinkedSections(PreviewWeapon weapon) =>
            AssetDatabase.Contains(weapon.Shooting.Config)
                ? new[] { new LinkedConfigSection(weapon.Shooting.Config, "Spread", nameof(ShootingWeaponConfig.Spread)) }
                : Array.Empty<LinkedConfigSection>();
    }
}
