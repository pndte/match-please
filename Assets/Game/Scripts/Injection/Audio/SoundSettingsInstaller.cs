using Bw.UseCases.Audio.View.Playback;
using Bw.UseCases.Character.View.Audio;
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
        [SerializeField] private WeaponSoundsConfig _weapon = new();
        [SerializeField] private MovementSoundsConfig _movement = new();
        [SerializeField] private CharacterSoundsConfig _character = new();

        public override void InstallBindings()
        {
            Container.BindInstance(_player).AsSingle();
            Container.BindInstance(_weapon).AsSingle();
            Container.BindInstance(_movement).AsSingle();
            Container.BindInstance(_character).AsSingle();
        }
    }
}
