using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Simulation;
using Zenject;

namespace Bw.Injection.Network
{
    public class LagCompensationInstaller<TState, TView> : Installer<IRuntimeSettings, LagCompensationInstaller<TState, TView>>
        where TState : struct
        where TView : class, IStateView<TState>
    {
        private readonly IRuntimeSettings _runtimeSettings;

        public LagCompensationInstaller(IRuntimeSettings runtimeSettings)
        {
            _runtimeSettings = runtimeSettings;
        }

        public override void InstallBindings()
        {
            if (_runtimeSettings.CurrentPeerType != PeerType.Server)
                return;

            Container.Bind<SimulationHistory<TState>>().AsSingle().NonLazy();
            Container.Bind<IBubbleHistory>().To<BubbleHistory>().AsSingle().NonLazy();
            Container.Bind<IRewindable>()
                .FromMethod(context => context.Container.Instantiate<SimulationRewinder<TState>>(
                    new object[] { context.Container.Instantiate(typeof(TView)) }))
                .AsSingle();
            Container.Bind<RewindableRegistrar>().AsSingle().NonLazy();
        }
    }
}
