using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Network;
using Bw.Injection.Network.Prediction;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Network.Guarding;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Mortality
{
    public sealed class GuardInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, GuardConfig, GuardInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;
        private readonly GuardConfig _config;

        public GuardInstaller(IRuntimeSettings runtimeSettings, INetEntriesSchemaBuilder netSchema, GuardConfig config)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
            _config = config;
        }

        public override void InstallBindings()
        {
            var state = _netSchema.DeclareResult<TickedState<GuardState>>(NetworkDelivery.UnreliableSequenced);
            Container.BindInstance(_config).AsSingle();

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind(typeof(IReadonlyGuard), typeof(IGuardCover), typeof(IGuardBlows), typeof(ISteppable<GuardStance>), typeof(IReadonlySimulation<GuardState>)).To<Guard>().AsSingle();
                    BindBlows();
                    Container.BindResultSender(state).WhenInjectedInto<PredictionStateBroadcaster<GuardState>>();
                    Container.Bind<PredictionStateBroadcaster<GuardState>>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind(typeof(IReadonlyGuard), typeof(IGuardCover), typeof(ISteppable<GuardStance>), typeof(IReadonlySimulation<GuardState>), typeof(IRestorable<GuardState>)).To<Guard>().AsSingle()
                        .WriterOnlyInto<IRestorable<GuardState>>(typeof(RollbackPart<GuardState>), typeof(ServerStateFollower<GuardState>));
                    Container.BindResultReceiver(state).WhenInjectedInto<ReplicatedStates<GuardState>>();
                    RollbackPartInstaller<GuardState>.Install(Container);
                    Container.Bind<IStateDivergence<GuardState>>().To<GuardDivergence>().AsSingle();
                    Container.Bind<GuardCoverRegistration>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_runtimeSettings.CurrentPeerType), _runtimeSettings.CurrentPeerType, null);
            }
        }

        private void BindBlows()
        {
            Container.Bind<GuardBlows>().AsSingle();
            Container.Bind<AffectableRegistration<IReadonlyMortal, Impacts>>()
                .FromMethod(context => context.Container.Instantiate<AffectableRegistration<IReadonlyMortal, Impacts>>(new object[] { context.Container.Resolve<GuardBlows>() }))
                .AsCached()
                .NonLazy();
            Container.Decorate<IEffectRules<Vitals, Impacts>>().With<GuardedVitalsRules>();
        }
    }
}
