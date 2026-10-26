using Bw.Entities.Network.Variables;
using Zenject;

namespace Bw.Injection.Network
{
    public static class NetworkInjectionExtensions
    {
        public static ConditionCopyNonLazyBinder BindRequestSender<TValue>(this DiContainer container,
            NetRequestDeclaration<TValue> declaration) =>
            container.Bind<INetRequestSender<TValue>>()
                .FromMethod(context => context.Container.Resolve<INetEntries>().RequestSender(declaration));

        public static ConditionCopyNonLazyBinder BindRequestReceiver<TValue>(this DiContainer container,
            NetRequestDeclaration<TValue> declaration) =>
            container.Bind<INetRequestReceiver<TValue>>()
                .FromMethod(context => context.Container.Resolve<INetEntries>().RequestReceiver(declaration));

        public static ConditionCopyNonLazyBinder BindResultSender<TValue>(this DiContainer container,
            NetResultDeclaration<TValue> declaration) =>
            container.Bind<INetResultSender<TValue>>()
                .FromMethod(context => context.Container.Resolve<INetEntries>().ResultSender(declaration));

        public static ConditionCopyNonLazyBinder BindResultReceiver<TValue>(this DiContainer container,
            NetResultDeclaration<TValue> declaration) =>
            container.Bind<INetResultReceiver<TValue>>()
                .FromMethod(context => context.Container.Resolve<INetEntries>().ResultReceiver(declaration));
    }
}
