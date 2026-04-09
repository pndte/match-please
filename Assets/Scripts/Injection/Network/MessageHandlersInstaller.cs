using System;
using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Codecs;
using Bw.Entities.Network.Routing;
using JetBrains.Lifetimes;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Network
{
    public class MessageHandlersInstaller : Installer<MessageHandlersInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind(typeof(INetworkRouter), typeof(IClientNetworkRouter)).To<NetworkRouter>().AsSingle();
            
            var senders = new Dictionary<Type, IMessageSender>(8);
            var receivers = new Dictionary<Type, IMessageReceiver>(8);
            
            RegisterHandlers();
            Container.BindInterfacesTo<MessageReceivers>().AsSingle().WithArguments(receivers);
            Container.BindInterfacesTo<MessageSenders>().AsSingle().WithArguments(senders);
            
            Container.Bind<MessagesHandler>().AsSingle().WithArguments(Lifetime.Eternal).NonLazy();
            return;

            void RegisterHandlers()
            {
                Register<int, IntCodec>();
                Register<float, FloatCodec>();
                Register<bool, BoolCodec>();
            }

            void Register<TValue, TCodec>()
                where TCodec : struct, INetworkSerializable, ICodec<TValue>
            {
                var key = typeof(TValue);

                if (receivers.ContainsKey(key) || senders.ContainsKey(key))
                    throw new Exception($"Handlers for '{key}' already registered.");

                receivers.Add(key, Container.Instantiate<MessageReceiver<TValue, TCodec>>());
                senders.Add(key, Container.Instantiate<MessageSender<TValue, TCodec>>());
            }
        }
    }
}