using System;
using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Routing;
using JetBrains.Lifetimes;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Network
{
    public partial class MessageHandlersInstaller : Installer<MessageHandlersInstaller>
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        public override void InstallBindings()
        {
            Container.Bind(typeof(IClientNetworkRouter), typeof(IServerNetworkRouter)).To<NetworkRouter>().AsSingle();

            var receivers = new Dictionary<Type, IMessageReceiver>(8);
            var senders = new Dictionary<Type, IMessageSender>(8);

            RegisterGeneratedMessageCodecs(receivers, senders);

            Container.BindInterfacesTo<MessageReceivers>().AsSingle().WithArguments(receivers);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Client:
                    Container.Bind<IClientSendersCollection>()
                        .To<SendersCollection>()
                        .AsSingle()
                        .WithArguments(senders);
                    break;
                case PeerType.Server:
                    Container.Bind<IServerSendersCollection>()
                        .To<SendersCollection>()
                        .AsSingle()
                        .WithArguments(senders);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(_runtimeSettings.CurrentPeerType),
                        _runtimeSettings.CurrentPeerType,
                        null);
            }

            Container.Bind<MessagesHandler>().AsSingle().WithArguments(Lifetime.Eternal).NonLazy(); //TODO: lifetime приложения
        }

        partial void RegisterGeneratedMessageCodecs(
            Dictionary<Type, IMessageReceiver> receivers,
            Dictionary<Type, IMessageSender> senders);

        private void RegisterCodec<TValue, TCodec>(
            Dictionary<Type, IMessageReceiver> receivers,
            Dictionary<Type, IMessageSender> senders)
            where TCodec : struct, INetworkSerializable, ICodec<TValue>
        {
            var key = typeof(TValue);
            if (receivers.ContainsKey(key) || senders.ContainsKey(key))
                throw new Exception($"Handlers for '{key}' already registered.");

            receivers.Add(key, Container.Instantiate<MessageReceiver<TValue, TCodec>>());
            senders.Add(key, Container.Instantiate<MessageSender<TValue, TCodec>>());
        }

        private void RegisterCodecRouting<TValue, TCodec, TRouting>( //TODO: добавить для него тоже автогенерацию
            Dictionary<Type, IMessageReceiver> receivers,
            Dictionary<Type, IMessageSender> senders)
            where TCodec : struct, INetworkSerializable, ICodec<TValue>
            where TRouting : CodecTargetRouting<TCodec>
        {
            var key = typeof(TValue);
            if (receivers.ContainsKey(key) || senders.ContainsKey(key))
                throw new Exception($"Handlers for '{key}' already registered.");

            receivers.Add(key, Container.Instantiate<MessageReceiver<TValue, TCodec>>());
            senders.Add(key, Container.Instantiate<TargetedMessageSender<TValue, TCodec>>(
                new object[] { Container.Instantiate<TRouting>() }));
        }
    }
}
