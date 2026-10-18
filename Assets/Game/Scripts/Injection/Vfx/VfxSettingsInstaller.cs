using Bw.UseCases.Character.View.Death;
using Bw.UseCases.Crates.View.Break;
using Bw.UseCases.Props.View;
using Bw.UseCases.Shooting.View.Impact;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Vfx
{
    [CreateAssetMenu(fileName = "VfxSettings", menuName = "Installers/VfxSettings")]
    public sealed class VfxSettingsInstaller : ScriptableObjectInstaller<VfxSettingsInstaller>
    {
        [SerializeField] private ShotImpactConfig _shotImpact = new();
        [SerializeField] private StrikeImpactConfig _strikeImpact = new();
        [SerializeField] private CharacterDeathViewConfig _characterDeath = new();
        [SerializeField] private CrateBreakConfig _crateBreak = new();
        [SerializeField] private PropVfxConfig _prop = new();

        public override void InstallBindings()
        {
            Container.BindInstance(_shotImpact).AsSingle();
            Container.BindInstance(_strikeImpact).AsSingle();
            Container.BindInstance(_characterDeath).AsSingle();
            Container.BindInstance(_crateBreak).AsSingle();
            Container.BindInstance(_prop).AsSingle();
        }
    }
}
