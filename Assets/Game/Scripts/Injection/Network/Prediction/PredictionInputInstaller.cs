using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
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

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Client:
                    BindClient(input, inputMargin);
                    break;
                case PeerType.Server:
                    BindServer(input, inputMargin);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindClient(NetRequestDeclaration<TickedInput<TInput>> input, NetResultDeclaration<int> inputMargin)
        {
            Container.BindRequestSender(input).WhenInjectedInto<PredictionInputSender<TInput>>();
            Container.BindResultReceiver(inputMargin).WhenInjectedInto<InputMarginClientHandler>();

            Container.Bind(typeof(IFedInputs<TInput>), typeof(PredictionInputSender<TInput>)).To<PredictionInputSender<TInput>>().AsSingle().NonLazy();
            Container.Bind<InputMarginClientHandler>().AsSingle().NonLazy();
        }

        private void BindServer(NetRequestDeclaration<TickedInput<TInput>> input, NetResultDeclaration<int> inputMargin)
        {
            Container.BindRequestReceiver(input).WhenInjectedInto(typeof(PredictionInputBuffer<TInput>), typeof(InputMarginServerHandler<TInput>));
            Container.BindResultSender(inputMargin).WhenInjectedInto<InputMarginServerHandler<TInput>>();

            Container.Bind<PredictionInputBuffer<TInput>>().AsSingle().NonLazy();
#if UNITY_EDITOR
            Container.Bind<InputGapLog<TInput>>().AsSingle().NonLazy(); //TODO: пропуски ввода считаются только в редакторе, поэтому в отладочных сборках сводок [Input] нет
#endif
            Container.Bind<BufferedInputSteps<TInput>>().AsSingle().NonLazy();
            Container.Bind<ControllerSampling<TInput>>().AsSingle().NonLazy();
            Container.Bind<InputMarginServerHandler<TInput>>().AsSingle().NonLazy();
        }
    }
}
