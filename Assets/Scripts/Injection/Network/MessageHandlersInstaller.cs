using System;
using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Codecs;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Network
{
    public class MessageHandlersInstaller : Installer<MessageHandlersInstaller>
    {
        public override void InstallBindings()
        {
            var senders = new Dictionary<Type, IMessageSender>(8);
            var receivers = new Dictionary<Type, IMessageReceiver>(8);

            RegisterReceivers();
            Container.BindInterfacesTo<MessageReceivers>().AsSingle().WithArguments(receivers);
            Container.Bind(typeof(INetworkRouter), typeof(IClientNetworkRouter)).To<NetworkRouter>().AsSingle();
            RegisterSenders();
            Container.BindInterfacesTo<MessageSenders>().AsSingle().WithArguments(senders);
            return;

            void RegisterReceivers()
            {
                RegisterReceiver<int, IntCodec>();
                RegisterReceiver<float, FloatCodec>();
                RegisterReceiver<bool, BoolCodec>();
            }

            void RegisterSenders() //TODO: как-то надо объединить
            {
                RegisterSender<int, IntCodec>();
                RegisterSender<float, FloatCodec>();
                RegisterSender<bool, BoolCodec>();
            }

            void RegisterReceiver<TValue, TCodec>()
                where TCodec : struct, INetworkSerializable, ICodec<TValue>
            {
                var key = typeof(TValue);

                if (receivers.ContainsKey(key))
                    throw new Exception($"Handlers for '{key}' already registered.");

                receivers.Add(key, Container.Instantiate<MessageReceiver<TValue, TCodec>>());
            }

            void RegisterSender<TValue, TCodec>()
                where TCodec : struct, INetworkSerializable, ICodec<TValue>
            {
                var key = typeof(TValue);

                if (senders.ContainsKey(key))
                    throw new Exception($"Handlers for '{key}' already registered.");

                senders.Add(key, Container.Instantiate<MessageSender<TValue, TCodec>>());
            }
        }
    }
}