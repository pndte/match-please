using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Variables;
using Bw.Injection.Network;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Weapon
{
    public sealed class CarriedWeaponsInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, CarriedWeaponsInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public CarriedWeaponsInstaller(IRuntimeSettings runtimeSettings, INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            var carried = _netSchema.DeclareResult<TickedState<CarriedWeapons>>(NetworkDelivery.UnreliableSequenced);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindResultSender(carried).WhenInjectedInto<CarriedWeaponsPublisher>();
                    Container.Bind<CarriedWeaponsReporter>().AsSingle();
                    Container.Bind<CarriedWeaponsPublisher>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.BindResultReceiver(carried).WhenInjectedInto<CarriedWeaponsReceiver>();
                    Container.Bind<IReplicatedStates<InventoryContent>>().To<CarriedWeaponsReceiver>().AsSingle().NonLazy();
                    Container.Bind<IStateDivergence<InventoryContent>>().To<ExactDivergence<InventoryContent>>().AsSingle();
                    Container.Bind<RollbackPart<InventoryContent>>().AsSingle();
                    Container.Bind<RollbackPartRegistrar<InventoryContent>>().AsSingle().NonLazy();
                    Container.Bind<ServerStateFollower<InventoryContent>>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_runtimeSettings.CurrentPeerType), _runtimeSettings.CurrentPeerType, null);
            }
        }
    }
}
