using System;
using System.Collections.Generic;
using Bw.UseCases.Combat.View.Recoil;
using Bw.UseCases.Combat.Weapon;
using UnityEditor;

namespace Bw.EditorTools.Shooting
{
    [CustomEditor(typeof(WeaponRecoilConfig))]
    public sealed class WeaponRecoilConfigEditor : WeaponFeelEditor
    {
        protected override PreviewWeapon PreviewedWeapon() =>
            PreviewWeapon.ForRecoil((WeaponRecoilConfig)target);

        protected override IReadOnlyList<LinkedConfigSection> LinkedSections(PreviewWeapon weapon) =>
            weapon.Shooting.Switch(
                gun => AssetDatabase.Contains(gun)
                    ? new[] { new LinkedConfigSection(gun, "Spread", nameof(ShootingWeaponConfig.Spread)) }
                    : Array.Empty<LinkedConfigSection>(),
                melee => Array.Empty<LinkedConfigSection>());
    }
}
