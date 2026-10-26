using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Variables;
using Bw.Entities.Simulation;
using Bw.Injection.Network.Prediction;
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
            WireRollbackStateInstaller<HandsState, HandsSlots>.Install(Container, _runtimeSettings, _netSchema);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    BindHands(typeof(IHands), typeof(IReadonlyHands), typeof(IReadonlySimulation<HandsState>), typeof(IRestorable<HandsState>), typeof(ISteppable<HandsInput>));
                    BindServer();
                    break;
                case PeerType.Client:
                    BindHands(typeof(IReadonlyHands), typeof(IReadonlySimulation<HandsState>), typeof(IRestorable<HandsState>), typeof(ISteppable<HandsInput>));
                    BindClient();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_runtimeSettings.CurrentPeerType), _runtimeSettings.CurrentPeerType, null);
            }
        }

        private void BindRules()
        {
            Container.Bind(typeof(IReadonlyWeaponInventory), typeof(IWeaponInventory), typeof(IRestorable<InventoryContent>)).To<CharacterInventory>().AsSingle()
                .WriterOnlyInto<IRestorable<InventoryContent>>(typeof(RollbackPart<InventoryContent>), typeof(ServerStateFollower<InventoryContent>));
            Container.Bind(typeof(ISimulator<HandsInput, HandsState>), typeof(HandsSimulator)).To<HandsSimulator>().AsSingle();
            Container.Bind<HandsControlsForm>().AsSingle();
            Container.Bind<IWireForm<HandsState, HandsSlots>>().To<HandsSlotsForm>().AsSingle();
        }

        private void BindHands(params Type[] roles)
        {
            Container.Bind(roles).To<Hands>().AsSingle().WithArguments(_oval)
                .WriterOnlyInto<IRestorable<HandsState>>(typeof(RollbackPart<HandsState>), typeof(ServerStateFollower<HandsState>));
            Container.Bind<DrawnWeapons>().AsSingle().NonLazy();
        }

        private void BindHold()
        {
            Container.Bind<HandsGrip>().AsSingle();
            Container.Bind<HandsHold>().AsSingle().NonLazy();
            Container.Decorate<IBodyPose>().With<HoldingBody>();
        }

        private void BindServer()
        {
            Container.Decorate<ISimulator<HandsInput, HandsState>>().With<BusyHandsGuard>();
            Container.Bind<IInputPolicy<HandsControls>>().To<HandsControlsPolicy>().AsSingle();
            Container.Bind<HandsOnlyDropper>().AsSingle().NonLazy();
        }

        private void BindClient()
        {
            Container.Bind<IStateDivergence<HandsState>>().To<ExactDivergence<HandsState>>().AsSingle();
            Container.Bind<CursorAim>().AsSingle().WithArguments(_oval.transform);
            Container.Bind<TriggerSampler>().AsSingle();
            Container.Bind<ReloadSampler>().AsSingle();
            Container.Bind<DrawControlSampler>().AsSingle();
            Container.Bind<IInputSampler<HandsControls>>().To<HandsControlsSampler>().AsSingle();
        }
    }
}
