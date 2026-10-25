using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Variables;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Network.Prediction
{
    public class PredictionInputInstaller<TInput> : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, PredictionInputInstaller<TInput>>
        where TInput : struct
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public PredictionInputInstaller(IRuntimeSettings runtimeSettings, INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            var input = _netSchema.DeclareRequest<TickedInput<TInput>>(NetworkDelivery.Unreliable);
            var inputMargin = _netSchema.DeclareResult<int>(NetworkDelivery.UnreliableSequenced);
            var relayed = _netSchema.DeclareResult<TickedInput<TInput>>(NetworkDelivery.UnreliableSequenced);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Client:
                    BindClient(input, inputMargin, relayed);
                    break;
                case PeerType.Server:
                    BindServer(input, inputMargin, relayed);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Container.Bind<InputSteps<TInput>>().AsSingle().NonLazy();
        }

        private void BindClient(
            NetRequestDeclaration<TickedInput<TInput>> input,
            NetResultDeclaration<int> inputMargin,
            NetResultDeclaration<TickedInput<TInput>> relayed)
        {
            Container.BindRequestSender(input).WhenInjectedInto<PredictionInputSender<TInput>>();
            Container.BindResultReceiver(inputMargin).WhenInjectedInto<InputMarginClientHandler>();
            Container.BindResultReceiver(relayed).WhenInjectedInto<RemoteInputs<TInput>>();

            Container.Bind(typeof(IReadonlyTickHistory<TInput>), typeof(ITickHistory<TInput>)).To<TickHistory<TInput>>()
                .FromMethod(context => new TickHistory<TInput>(context.Container.Resolve<WorldRollbackConfig>().HistoryTicks))
                .AsSingle()
                .WriterOnlyInto<ITickHistory<TInput>>(typeof(PredictionInputSender<TInput>));
            Container.Bind<PredictionInputSender<TInput>>().AsSingle().NonLazy();
            Container.Bind(typeof(IRemoteInputs<TInput>), typeof(RemoteInputs<TInput>)).To<RemoteInputs<TInput>>().AsSingle();
            Container.Bind<RemoteInputsRegistration<TInput>>().AsSingle().NonLazy();
            Container.Bind<IStepInputs<TInput>>().To<PredictedInputs<TInput>>().AsSingle();
            Container.Bind<InputMarginClientHandler>().AsSingle().NonLazy();
        }

        private void BindServer(
            NetRequestDeclaration<TickedInput<TInput>> input,
            NetResultDeclaration<int> inputMargin,
            NetResultDeclaration<TickedInput<TInput>> relayed)
        {
            Container.BindRequestReceiver(input).WhenInjectedInto(typeof(PredictionInputBuffer<TInput>), typeof(InputMarginServerHandler<TInput>));
            Container.BindResultSender(inputMargin).WhenInjectedInto<InputMarginServerHandler<TInput>>();
            Container.BindResultSender(relayed).WhenInjectedInto<InputRelay<TInput>>();

            Container.Bind<PredictionInputBuffer<TInput>>().AsSingle().NonLazy();
#if UNITY_EDITOR
            Container.Bind<InputGapLog<TInput>>().AsSingle().NonLazy(); //TODO: пропуски ввода считаются только в редакторе, поэтому в отладочных сборках сводок [Input] нет
#endif
            Container.Bind(typeof(IStepInputs<TInput>), typeof(DriverInputs<TInput>)).To<DriverInputs<TInput>>().AsSingle();
            Container.Bind<InputRelay<TInput>>().AsSingle().NonLazy();
            Container.Bind<InputMarginServerHandler<TInput>>().AsSingle().NonLazy();
        }
    }
}
