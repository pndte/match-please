using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Injection.Network;
using Bw.UseCases.Shooting.View.Adapters;
using Bw.UseCases.Shooting.Weapon.Network.Requests;
using Bw.UseCases.Shooting.Weapon.Pickup;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Weapon
{
    public class WeaponPickupInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, Transform, WeaponPickupInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;
        private readonly Transform _body;

        public WeaponPickupInstaller(IRuntimeSettings runtimeSettings, INetEntriesSchemaBuilder netSchema, Transform body)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
            _body = body;
        }

        public override void InstallBindings()
        {
            var pickup = _netSchema.DeclareRequest<WeaponPickup>(NetworkDelivery.Reliable);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindRequestReceiver(pickup).WhenInjectedInto<WeaponPickupServerHandler>();
                    Container.Bind<WeaponPickupServerHandler>().AsSingle().WithArguments(_body).NonLazy();
                    break;
                case PeerType.Client:
                    Container.BindRequestSender(pickup).WhenInjectedInto<WeaponPickupClientHandler>();
                    Container.BindInterfacesTo<PickupTargeting>().AsSingle().WithArguments(_body);
                    Container.Bind<WeaponPickupClientHandler>().AsSingle().NonLazy();
                    BindClientVisuals();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BindClientVisuals()
        {
            Container.Bind<PickupTargetHighlight>().AsSingle().NonLazy();
        }
    }
}
