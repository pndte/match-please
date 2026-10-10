using System;
using System.Collections.Generic;
using System.Linq;
using Bw.Injection.Weapon;
using Bw.UseCases.Shooting;
using Bw.UseCases.Shooting.Melee;
using Bw.UseCases.Shooting.View.Recoil;
using Bw.UseCases.Shooting.Weapon;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.EditorTools.Shooting
{
    public sealed class PreviewWeapon : IDisposable
    {
        private const string StandInName = "no weapon prefab uses this config: a stand-in gun";
        private const float StandInCooldown = 0.1f;
        private const int StandInAmmo = 30;
        private const float StandInOrbit = 0.4f;

        private static readonly Vector2 StandInMuzzle = new(1.1f, 0f);

        public string Name { get; }
        public IReadOnlyList<PreviewPart> Parts { get; }
        public Vector2 Muzzle { get; }
        public float OrbitRadius { get; }
        public PreviewShooting Shooting { get; }
        public WeaponRecoilConfig Recoil { get; }

        private readonly Object[] _standIns;

        private PreviewWeapon(
            string name,
            IReadOnlyList<PreviewPart> parts,
            Vector2 muzzle,
            float orbitRadius,
            PreviewShooting shooting,
            WeaponRecoilConfig recoil,
            params Object[] standIns)
        {
            Name = name;
            Parts = parts;
            Muzzle = muzzle;
            OrbitRadius = orbitRadius;
            Shooting = shooting;
            Recoil = recoil;
            _standIns = standIns;
        }

        public static PreviewWeapon ForRecoil(WeaponRecoilConfig recoil)
        {
            foreach (var prefab in Prefabs())
            {
                if (prefab.TryGetComponent<WeaponInstaller>(out var gun) && IsLinked(gun, WeaponInstaller.RecoilConfigField, recoil))
                    return Gun(gun);

                if (prefab.TryGetComponent<MeleeWeaponInstaller>(out var melee) && IsLinked(melee, MeleeWeaponInstaller.RecoilConfigField, recoil))
                    return Melee(melee);
            }

            var ammo = ScriptableObject.CreateInstance<AmmoConfig>();
            ammo.Max = StandInAmmo;
            var shooting = ScriptableObject.CreateInstance<ShootingWeaponConfig>();
            shooting.AmmoSettings = ammo;
            shooting.ShootCooldown = StandInCooldown;
            Hide(ammo, shooting);
            return new PreviewWeapon(StandInName, Array.Empty<PreviewPart>(), StandInMuzzle, StandInOrbit, PreviewShooting.Gun(shooting), recoil, ammo, shooting);
        }

        public static PreviewWeapon ForShooting(ShootingWeaponConfig shooting)
        {
            foreach (var prefab in Prefabs())
                if (prefab.TryGetComponent<WeaponInstaller>(out var gun) && IsLinked(gun, WeaponInstaller.ShootingConfigField, shooting))
                    return Gun(gun);

            var recoil = ScriptableObject.CreateInstance<WeaponRecoilConfig>();
            Hide(recoil);
            return new PreviewWeapon(StandInName, Array.Empty<PreviewPart>(), StandInMuzzle, StandInOrbit, PreviewShooting.Gun(shooting), recoil, recoil);
        }

        public void Dispose()
        {
            foreach (var standIn in _standIns)
                Object.DestroyImmediate(standIn);
        }

        private static IEnumerable<GameObject> Prefabs() =>
            AssetDatabase.FindAssets("t:Prefab")
                .Select(guid => AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid)));

        private static bool IsLinked(Component installer, string field, Object config)
        {
            using var serialized = new SerializedObject(installer);
            return serialized.FindProperty(field).objectReferenceValue == config;
        }

        private static PreviewWeapon Gun(WeaponInstaller gun)
        {
            using var installer = new SerializedObject(gun);
            var muzzle = (Transform)installer.FindProperty(WeaponInstaller.MuzzleField).objectReferenceValue;
            var shooting = (ShootingWeaponConfig)installer.FindProperty(WeaponInstaller.ShootingConfigField).objectReferenceValue;
            return Read(
                gun.gameObject,
                (Transform)installer.FindProperty(WeaponInstaller.VisualField).objectReferenceValue,
                (WeaponRotationConfig)installer.FindProperty(WeaponInstaller.RotationConfigField).objectReferenceValue,
                (WeaponRecoilConfig)installer.FindProperty(WeaponInstaller.RecoilConfigField).objectReferenceValue,
                muzzle.localPosition,
                PreviewShooting.Gun(shooting));
        }

        private static PreviewWeapon Melee(MeleeWeaponInstaller melee)
        {
            using var installer = new SerializedObject(melee);
            var swing = (MeleeWeaponConfig)installer.FindProperty(MeleeWeaponInstaller.MeleeConfigField).objectReferenceValue;
            return Read(
                melee.gameObject,
                (Transform)installer.FindProperty(MeleeWeaponInstaller.VisualField).objectReferenceValue,
                (WeaponRotationConfig)installer.FindProperty(MeleeWeaponInstaller.RotationConfigField).objectReferenceValue,
                (WeaponRecoilConfig)installer.FindProperty(MeleeWeaponInstaller.RecoilConfigField).objectReferenceValue,
                Vector2.zero,
                PreviewShooting.Melee(swing));
        }

        private static PreviewWeapon Read(
            GameObject prefab,
            Transform visual,
            WeaponRotationConfig rotation,
            WeaponRecoilConfig recoil,
            Vector2 muzzle,
            PreviewShooting shooting)
        {
            var parts = visual.GetComponentsInChildren<SpriteRenderer>(true)
                .Where(renderer => renderer.enabled && renderer.sprite != null && ActiveUnder(renderer.transform, visual))
                .OrderBy(renderer => renderer.sortingOrder)
                .Select(renderer => new PreviewPart(renderer.sprite, PartMatrix(visual, renderer)))
                .ToList();

            return new PreviewWeapon(prefab.name, parts, muzzle, rotation.MaxOrbitRadius, shooting, recoil);
        }

        private static void Hide(params Object[] standIns)
        {
            foreach (var standIn in standIns)
                standIn.hideFlags = HideFlags.HideAndDontSave;
        }

        private static Matrix4x4 PartMatrix(Transform visual, SpriteRenderer renderer) =>
            visual.worldToLocalMatrix * renderer.transform.localToWorldMatrix *
            Matrix4x4.Scale(new Vector3(renderer.flipX ? -1f : 1f, renderer.flipY ? -1f : 1f, 1f));

        private static bool ActiveUnder(Transform part, Transform visual)
        {
            for (var current = part; current != visual; current = current.parent)
                if (!current.gameObject.activeSelf)
                    return false;

            return true;
        }
    }
}
