using Setup;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public class RuntimeSettingsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IRuntimeSettings>().To<RuntimeSettings>().AsSingle().NonLazy(); // TODO: сделать инициализацию явной, не через NetworkAutoStart
        }
    }
}