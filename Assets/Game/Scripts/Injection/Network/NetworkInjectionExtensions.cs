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

        public static ConditionCopyNonLazyBinder BindRequestSender<TValue>(this DiContainer container,
            NetRequestDeclaration<TValue> declaration) =>
            container.Bind<INetRequestSender<TValue>>()
                .FromMethod(context => context.Container.Resolve<INetEntries>().Sender(declaration));

        public static ConditionCopyNonLazyBinder BindRequestReceiver<TValue>(this DiContainer container,
            NetRequestDeclaration<TValue> declaration) =>
            container.Bind<INetRequestReceiver<TValue>>()
                .FromMethod(context => context.Container.Resolve<INetEntries>().Receiver(declaration));

        public static ConditionCopyNonLazyBinder BindResultSender<TValue>(this DiContainer container,
            NetResultDeclaration<TValue> declaration) =>
            container.Bind<INetResultSender<TValue>>()
                .FromMethod(context => context.Container.Resolve<INetEntries>().Sender(declaration));

        public static ConditionCopyNonLazyBinder BindResultReceiver<TValue>(this DiContainer container,
            NetResultDeclaration<TValue> declaration) =>
            container.Bind<INetResultReceiver<TValue>>()
                .FromMethod(context => context.Container.Resolve<INetEntries>().Receiver(declaration));
    }
}
