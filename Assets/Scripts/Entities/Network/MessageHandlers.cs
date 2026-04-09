using System;
using System.Collections.Generic;

namespace Bw.Entities.Network
{
    public interface IMessageSenders
    {
        public IReadOnlyDictionary<Type, IMessageSender> ByType { get; }
    }

    public interface IMessageReceivers
    {
        public IReadOnlyDictionary<Type, IMessageReceiver> ByType { get; }
    }

    public class MessageReceivers : IMessageReceivers
    {
        public IReadOnlyDictionary<Type, IMessageReceiver> ByType { get; }

        public MessageReceivers(IReadOnlyDictionary<Type, IMessageReceiver> receiversByType)
        {
            ByType = receiversByType;
        }
    }
    
    public class MessageSenders : IMessageSenders
    {
        public IReadOnlyDictionary<Type, IMessageSender> ByType { get; }

        public MessageSenders(IReadOnlyDictionary<Type, IMessageSender> sendersByType)
        {
            ByType = sendersByType;
        }
    }
}