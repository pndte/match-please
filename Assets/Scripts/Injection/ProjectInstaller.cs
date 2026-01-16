using DefaultNamespace;
using Setup;
using UnityEngine;
using Zenject;

namespace Injection
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IRuntimeSettings>().To<RuntimeSettings>().AsSingle(); // TODO: сделать инициализацию явной, не через NetworkAutoStart
            Container.BindInterfacesTo<DiContainer>().FromInstance(Container).AsSingle();
        }
    }
}

