using System;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Entities.Network.Routing
{
    public class MessagesHandler : INetSyncVisitor
    {
        private readonly INetworkHolder _networkHolder;
        private readonly IMessageReceivers _messageReceivers;

        private FastBufferReader _currentReader;
        private ulong _currentSender;

        public MessagesHandler(
            Lifetime lifetime,
            INetworkHolder networkHolder,
            IMessageReceivers messageReceivers)
        {
            _messageReceivers = messageReceivers;
            _networkHolder = networkHolder;

            _networkHolder.NetworkManager.AdviseNotNull(lifetime, network =>
            {
                var messaging = network.CustomMessagingManager;
                messaging.OnUnnamedMessage += HandleUnnamedMessage;
                lifetime.OnTermination(() => messaging.OnUnnamedMessage -= HandleUnnamedMessage);
            });
        }

        private void HandleUnnamedMessage(ulong senderClientId, FastBufferReader reader)
        {
            reader.ReadNetworkSerializable(out NetworkMessageHeader header);

            if (!_networkHolder.SpawnManager().SpawnedObjects.TryGetValue(header.NetworkObjectId, out var netObj))
            {
                Debug.LogWarning(
                    $"[MessagesHandler] No spawned network object with id '{header.NetworkObjectId.ToString()}' " +
                    $"(varId={header.VarId.ToString()}, sender={senderClientId.ToString()}). Message ignored.");
                return;
            }

            if (!netObj.TryGetComponent<INetworkLifetimedObject>(out var targetObject))
            {
                throw new Exception(
                    $"[MessagesHandler] Network object '{header.NetworkObjectId.ToString()}:{netObj.name}' has no {nameof(INetworkLifetimedObject)}. Message ignored.");
            }

            var inbox = targetObject.Inbox
                ?? throw new InvalidOperationException(
                    $"[MessagesHandler] Network object '{header.NetworkObjectId.ToString()}:{netObj.name}' has no net inbox, it was instantiated without its DI context.");

            _currentReader = reader;
            _currentSender = senderClientId;
            inbox.Receive(senderClientId, header, this); // the entry calls VisitProperty or VisitSignal down below
        }

        void INetSyncVisitor.VisitProperty<T>(INetProperty<T> property) =>
            ReceiverOf<T>().ReceiveProperty(ref _currentReader, property);

        void INetSyncVisitor.VisitSignal<T>(INetSignal<T> entry) =>
            ReceiverOf<T>().ReceiveSignal(ref _currentReader, entry, _currentSender);

        private IMessageReceiver<T> ReceiverOf<T>() =>
            _messageReceivers.ByType.TryGetValue(typeof(T), out var receiver)
                ? (IMessageReceiver<T>)receiver
                : throw new InvalidOperationException(
                    $"[MessagesHandler] No codec receives {typeof(T).Name}: its codec is not registered (Tools → Network → Regenerate message codec registrations).");
    }
}
