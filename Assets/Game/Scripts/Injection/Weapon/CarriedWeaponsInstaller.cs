using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Injection.Network;
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
            var carried = _netSchema.DeclareResult<CarriedWeapons>(NetworkDelivery.ReliableSequenced);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindResultSender(carried).WhenInjectedInto(typeof(CarriedWeaponsPublisher), typeof(LateJoinCarriedSender));
                    Container.Bind<CarriedWeaponsReport>().AsSingle();
                    Container.Bind<CarriedWeaponsPublisher>().AsSingle().NonLazy();
                    Container.Bind<LateJoinCarriedSender>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.BindResultReceiver(carried).WhenInjectedInto<CarriedWeaponsReceiver>();
                    Container.Bind<ICarriedReplica>().To<CarriedWeaponsReceiver>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_runtimeSettings.CurrentPeerType), _runtimeSettings.CurrentPeerType, null);
            }
        }
    }
}
