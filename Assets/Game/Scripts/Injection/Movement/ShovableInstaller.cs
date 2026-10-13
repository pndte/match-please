using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Network.Prediction;
using Bw.UseCases.Movement.Physics;
using Bw.UseCases.Movement.Physics.Abstractions;
using Zenject;

namespace Bw.Injection.Movement
{
    public class ShovableInstaller<TState> : Installer<IRuntimeSettings, ShovableInstaller<TState>>
        where TState : struct, IBodyState<TState>
    {
        private readonly IRuntimeSettings _runtimeSettings;

        public ShovableInstaller(IRuntimeSettings runtimeSettings)
        {
            _runtimeSettings = runtimeSettings;
        }

        public override void InstallBindings()
        {
            Container.Bind<ShovableRegistration>().AsSingle().NonLazy();

            if (_runtimeSettings.CurrentPeerType != PeerType.Client)
                return;

            Container.Bind(typeof(IShovable), typeof(IPredictedBody), typeof(IPredictedShove), typeof(IShoveHistory)).To<PredictedShovable<TState>>().AsSingle().NonLazy();
            Container.Bind<PredictedBodyRegistration>().AsSingle().NonLazy();
            BindClientVisuals();
        }

        private void BindClientVisuals() =>
            Container.Decorate<IStateView<TState>>().With<ShovedStateView<TState>>();
    }
}
