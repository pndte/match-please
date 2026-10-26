using System.Linq;
using UnityEngine.AddressableAssets;
using Zenject;

namespace Bw.Injection
{
    public sealed class AddressableSettingsInstaller : MonoInstaller
    {
        public const string Label = "settings";

        public override void InstallBindings()
        {
            var installers = Addressables.LoadAssetsAsync<ScriptableObjectInstaller>(Label).WaitForCompletion();
            foreach (var installer in installers.OrderBy(installer => installer.name))
            {
                Container.Inject(installer);
                installer.InstallBindings();
            }
        }
    }
}
