using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Entities.Players;
using Bw.UseCases.Character;
using Bw.UseCases.Character.Network;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection
{
    public sealed class NicknameInstaller : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, NicknameInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public NicknameInstaller(IRuntimeSettings runtimeSettings, INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            var nickname = _netSchema.DeclareProperty(NicknameRules.Fallback, NetworkDelivery.Reliable, NetworkPermissions.Server);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<NicknamePublisher>().AsSingle().WithArguments(nickname).NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind(typeof(IReadonlyNickname), typeof(INickname)).To<Nickname>().AsSingle()
                        .WriterOnlyInto<INickname>(typeof(NicknameReceiver));
                    Container.Bind<NicknameReceiver>().AsSingle().WithArguments(nickname).NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_runtimeSettings.CurrentPeerType), _runtimeSettings.CurrentPeerType, null);
            }
        }
    }
}
