using System;
using System.Collections.Generic;

namespace Bw.Entities.Network
{
    public interface IClientSendersCollection
    {
        IClientMessageSender<T> Get<T>();
    }

    public interface IServerSendersCollection
    {

        IServerMessageSender<T> Get<T>();
    }

    public interface IMessageReceivers
    {
        IReadOnlyDictionary<Type, IMessageReceiver> ByType { get; }
    }

    public sealed class MessageReceivers : IMessageReceivers
    {
        public IReadOnlyDictionary<Type, IMessageReceiver> ByType { get; }

        public MessageReceivers(IReadOnlyDictionary<Type, IMessageReceiver> receiversByType)
        {
            ByType = receiversByType;
        }
    }

    public sealed class SendersCollection : IClientSendersCollection, IServerSendersCollection
    {
        private readonly IReadOnlyDictionary<Type, IMessageSender> _byType;

        public SendersCollection(IReadOnlyDictionary<Type, IMessageSender> sendersByType)
        {
            _byType = sendersByType;
        }

        IClientMessageSender<T> IClientSendersCollection.Get<T>() =>
            (IMessageSender<T>)_byType[typeof(T)];

        IServerMessageSender<T> IServerSendersCollection.Get<T>() =>
            (IMessageSender<T>)_byType[typeof(T)];
    }
}
