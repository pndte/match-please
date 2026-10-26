using Bw.Entities.Extensions;
using JetBrains.Lifetimes;
using Zenject;

namespace Bw.Injection
{
    public class LifetimeInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<Lifetime>().FromMethod(() => Container.Resolve<Context>().gameObject.Lifetime()).AsSingle();
        }
    }
}
