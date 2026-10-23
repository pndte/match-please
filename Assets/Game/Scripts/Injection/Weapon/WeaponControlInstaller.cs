using System;
using Bw.Entities;
using Bw.Entities.Network;
using Bw.UseCases.Combat.Weapon.Network;
using Zenject;

namespace Bw.Injection.Weapon
{
    public sealed class WeaponControlInstaller : Installer<IRuntimeSettings, WeaponControlInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;

        public WeaponControlInstaller(IRuntimeSettings runtimeSettings)
        {
            _runtimeSettings = runtimeSettings;
        }

        public override void InstallBindings()
        {
            Container.Bind<IControlledBy>().To<UseCases.ControlledBy>().AsSingle();
            Container.Bind<WeaponControlRegistration>().AsSingle().NonLazy();

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<IControllingClients>().To<ControllingClients>().AsSingle();
                    Container.Bind<WeaponClientsRegistration>().AsSingle().NonLazy();
                    break;
                case PeerType.Client:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_runtimeSettings.CurrentPeerType), _runtimeSettings.CurrentPeerType, null);
            }
        }
    }
}
