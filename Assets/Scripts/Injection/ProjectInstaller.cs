using Setup;
using Zenject;

namespace Injection
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IRuntimeSettings>().To<RuntimeSettings>().AsSingle(); // TODO: сделать инициализацию явной, не через NetworkAutoStart
        }
    }
}

