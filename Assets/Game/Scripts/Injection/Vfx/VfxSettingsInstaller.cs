using Bw.UseCases.Shooting.View.Impact;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Vfx
{
    [CreateAssetMenu(fileName = "VfxSettings", menuName = "Installers/VfxSettings")]
    public sealed class VfxSettingsInstaller : ScriptableObjectInstaller<VfxSettingsInstaller>
    {
        [SerializeField] private ShotImpactConfig _shotImpact = new();

        public override void InstallBindings()
        {
            Container.BindInstance(_shotImpact).AsSingle();
        }
    }
}
