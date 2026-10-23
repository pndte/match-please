using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Injection.Network;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Weapon
{
    public sealed class FlashlightInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, FlashlightInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public FlashlightInstaller(IRuntimeSettings runtimeSettings, INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            var @switch = _netSchema.DeclareRequest<bool>(NetworkDelivery.Reliable);
            var lit = _netSchema.DeclareProperty(false, NetworkDelivery.Reliable, NetworkPermissions.Server);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind(typeof(IReadonlyFlashlight), typeof(IFlashlight)).To<Flashlight>().AsSingle()
                        .WriterOnlyInto<IFlashlight>(typeof(FlashlightServerHandler));
                    Container.BindRequestReceiver(@switch).WhenInjectedInto<FlashlightServerHandler>();
                    Container.Bind<FlashlightServerHandler>().AsSingle().NonLazy();
                    Container.Bind<FlashlightPublisher>().AsSingle().WithArguments(lit).NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind(typeof(IReadonlyFlashlight), typeof(IFlashlight)).To<Flashlight>().AsSingle()
                        .WriterOnlyInto<IFlashlight>(typeof(FlashlightReceiver));
                    Container.BindRequestSender(@switch).WhenInjectedInto<FlashlightClientHandler>();
                    Container.Bind<FlashlightReceiver>().AsSingle().WithArguments(lit).NonLazy();
                    Container.Bind<FlashlightClientHandler>().AsSingle().NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_runtimeSettings.CurrentPeerType), _runtimeSettings.CurrentPeerType, null);
            }
        }
    }
}
