using System;
using Bw.UseCases.Audio.View.Playback;
using Bw.UseCases.Character.View.Audio;
using Bw.UseCases.Crates.View.Audio;
using Bw.UseCases.Movement.View.Audio;
using Bw.UseCases.Shooting.View.Audio;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Audio
{
    [CreateAssetMenu(fileName = "SoundSettings", menuName = "Installers/SoundSettings")]
    public sealed class SoundSettingsInstaller : ScriptableObjectInstaller<SoundSettingsInstaller>
    {
        [SerializeField] private SoundPlayerConfig _player = new();
        [SerializeField] private WeaponSoundsConfig[] _weapons = Array.Empty<WeaponSoundsConfig>();
        [SerializeField] private ShotImpactSoundsConfig _shotImpact = new();
        [SerializeField] private MeleeSoundsConfig[] _melee = Array.Empty<MeleeSoundsConfig>();
        [SerializeField] private MovementSoundsConfig _movement = new();
        [SerializeField] private CharacterSoundsConfig _character = new();
        [SerializeField] private CrateSoundsConfig _crate = new();

        public override void InstallBindings()
        {
            Container.BindInstance(_player).AsSingle();
            Container.Bind<WeaponSoundsCatalog>().AsSingle().WithArguments(_weapons);
            Container.BindInstance(_shotImpact).AsSingle();
            Container.Bind<MeleeSoundsCatalog>().AsSingle().WithArguments(_melee);
            Container.BindInstance(_movement).AsSingle();
            Container.BindInstance(_character).AsSingle();
            Container.BindInstance(_crate).AsSingle();
        }
    }
}
