using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Network.Prediction;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Hands.Network;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Movement.Physics.Abstractions;
using Zenject;

namespace Bw.Injection
{
    public sealed class HandsInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, HoldOval, HandsInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;
        private readonly HoldOval _oval;

        public HandsInstaller(
            IRuntimeSettings runtimeSettings,
            INetEntriesSchemaBuilder netSchema,
            HoldOval oval)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
            _oval = oval;
        }

        public override void InstallBindings()
        {
            BindRules();
            BindHold();
            WirePredictionStateInstaller<CharacterControls, HandsState, HandsSlots>.Install(Container, _runtimeSettings, _netSchema);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    BindHands(typeof(IHands), typeof(IReadonlyHands), typeof(IReadonlySimulation<HandsState>), typeof(ISimulation<HandsState>), typeof(ISteppable<HandsInput>));
                    BindServer();
                    break;
                case PeerType.Client:
                    BindHands(typeof(IReadonlyHands), typeof(IReadonlySimulation<HandsState>), typeof(ISimulation<HandsState>), typeof(ISteppable<HandsInput>));
                    BindClient();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_runtimeSettings.CurrentPeerType), _runtimeSettings.CurrentPeerType, null);
            }
        }

        private void BindRules()
        {
            Container.Bind(typeof(IReadonlyWeaponInventory), typeof(IWeaponInventory)).To<CharacterInventory>().AsSingle();
            Container.Bind(typeof(ISimulator<HandsInput, HandsState>), typeof(HandsSimulator)).To<HandsSimulator>().AsSingle();
            Container.Bind<HandsControlsForm>().AsSingle();
            Container.Bind<IWireForm<HandsState, HandsSlots>>().To<HandsSlotsForm>().AsSingle();
        }

        private void BindHands(params Type[] roles)
        {
            Container.Bind(roles).To<Hands>().AsSingle().WithArguments(_oval)
                .WriterOnlyInto<ISimulation<HandsState>>(typeof(PredictionReconciler<CharacterControls, HandsState>), typeof(HandsStateView));
            Container.Bind<HandsDraw>().AsSingle().NonLazy();
            Container.Bind<DrawnWeaponControl>().AsSingle().NonLazy();
        }

        private void BindHold()
        {
            Container.Bind<HandsGrip>().AsSingle();
            Container.Bind<HandsHold>().AsSingle().NonLazy();
            Container.Decorate<IMovementBody>().With<HoldingBody>();
        }

        private void BindServer()
        {
            Container.Decorate<ISimulator<HandsInput, HandsState>>().With<BusyHandsGuard>();
            Container.Bind<IInputPolicy<HandsControls>>().To<HandsControlsPolicy>().AsSingle();
            Container.Bind<HandsOnlyDrop>().AsSingle().NonLazy();
            Container.Bind<DrawnWeaponClients>().AsSingle().NonLazy();
        }

        private void BindClient()
        {
            Container.Bind<ISimulator<CharacterControls, HandsState>>().To<ControlledHands>().AsSingle();
            Container.Bind<IStateView<HandsState>>().To<HandsStateView>().AsSingle();
            Container.Bind<CursorAim>().AsSingle().WithArguments(_oval.transform);
            Container.Bind<TriggerSampler>().AsSingle();
            Container.Bind<ReloadSampler>().AsSingle();
            Container.Bind<DrawControlSampler>().AsSingle();
            Container.Bind<IInputSampler<HandsControls>>().To<HandsControlsSampler>().AsSingle();
            Container.Bind<GripEcho>()
                .FromMethod(context => context.Container.Instantiate<GripEcho>(new object[] { context.Container.Instantiate<HandsGrip>() }))
                .AsSingle()
                .NonLazy();
        }
    }
}
