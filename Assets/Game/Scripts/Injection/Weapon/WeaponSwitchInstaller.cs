using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Injection.Network;
using Bw.UseCases.Shooting.Weapon.Network.Requests;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Weapon
{
    public class WeaponSwitchInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, WeaponSwitchInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public WeaponSwitchInstaller(IRuntimeSettings runtimeSettings, INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            var weaponSwitch = _netSchema.DeclareRequest<WeaponSwitch>(NetworkDelivery.Reliable);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.BindRequestReceiver(weaponSwitch).WhenInjectedInto<WeaponSwitchServerHandler>();
                    Container.Bind<WeaponSwitchServerHandler>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    Container.BindRequestSender(weaponSwitch).WhenInjectedInto<WeaponSwitchClientHandler>();
                    Container.Bind<WeaponSwitchClientHandler>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
