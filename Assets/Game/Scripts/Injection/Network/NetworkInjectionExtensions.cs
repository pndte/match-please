using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using Zenject;

namespace Bw.Injection.Network
{
    public static class NetworkInjectionExtensions
    {
        public static void BindNetPropertyFor<TValue, TDestination>(this DiContainer container,
            NetPropertyDeclaration<TValue> declaration)
        {
            container.Bind(typeof(IViewableProperty<TValue>), typeof(INetProperty<TValue>))
                .FromMethod(context => context.Container.Resolve<INetEntries>().Get(declaration))
                .WhenInjectedInto<TDestination>();
        }

        public static void BindNetSignalFor<TValue, TDestination>(this DiContainer container,
            NetSignalDeclaration<TValue> declaration)
        {
            container.Bind(typeof(ISignal<TValue>), typeof(INetSignal<TValue>))
                .FromMethod(context => context.Container.Resolve<INetEntries>().Get(declaration))
                .WhenInjectedInto<TDestination>();
        }
    }
}
