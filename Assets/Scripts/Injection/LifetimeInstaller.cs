using JetBrains.Lifetimes;
using Zenject;

namespace Bw.Injection
{
    public class LifetimeInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<Lifetime>().FromInstance(Lifetime.Eternal).AsSingle();
        }
    }
}