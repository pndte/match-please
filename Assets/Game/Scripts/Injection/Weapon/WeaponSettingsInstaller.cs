using Bw.UseCases.Shooting.View.Highlight;
using Bw.UseCases.Shooting.Weapon.Pickup;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Weapon
{
    [CreateAssetMenu(fileName = "WeaponSettings", menuName = "Installers/WeaponSettings")]
    public sealed class WeaponSettingsInstaller : ScriptableObjectInstaller<WeaponSettingsInstaller>
    {
        [SerializeField] private WeaponPickupConfig _pickup = new();
        [SerializeField] private WeaponHighlightConfig _highlight = new();

        public override void InstallBindings()
        {
            Container.BindInstance(_pickup).AsSingle();
            Container.BindInstance(_highlight).AsSingle();
        }
    }
}
