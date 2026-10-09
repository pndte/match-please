using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Variables;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Network.Prediction
{
    public class EventPredictionInitiatorInstaller<TInitiator, TTarget, TEffect>
        : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, EventPredictionInitiatorInstaller<TInitiator, TTarget, TEffect>>
        where TInitiator : class
        where TTarget : class
        where TEffect : struct
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public EventPredictionInitiatorInstaller(
            IRuntimeSettings runtimeSettings,
            INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            var report = _netSchema.DeclareResult<OutcomeReport<TEffect>>(NetworkDelivery.Reliable);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindResultSender(report).WhenInjectedInto<OutcomeDispatcher<TInitiator, TTarget, TEffect>>();
                    Container.Bind<OutcomeDispatcher<TInitiator, TTarget, TEffect>>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.BindResultReceiver(report).WhenInjectedInto<OutcomeReceiver<TTarget, TEffect>>();
                    Container.BindInterfacesAndSelfTo<OutcomePrediction<TTarget, TEffect>>().AsSingle();
                    Container.Bind<OutcomeReceiver<TTarget, TEffect>>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
