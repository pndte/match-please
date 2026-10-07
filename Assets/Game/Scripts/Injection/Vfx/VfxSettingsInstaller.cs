using Bw.UseCases.Character.View.Death;
using Bw.UseCases.Shooting.View.Impact;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Vfx
{
    [CreateAssetMenu(fileName = "VfxSettings", menuName = "Installers/VfxSettings")]
    public sealed class VfxSettingsInstaller : ScriptableObjectInstaller<VfxSettingsInstaller>
    {
        [SerializeField] private ShotImpactConfig _shotImpact = new();
        [SerializeField] private CharacterDeathViewConfig _characterDeath = new();

        public override void InstallBindings()
        {
            Container.BindInstance(_shotImpact).AsSingle();
            Container.BindInstance(_characterDeath).AsSingle();
        }
    }
}
