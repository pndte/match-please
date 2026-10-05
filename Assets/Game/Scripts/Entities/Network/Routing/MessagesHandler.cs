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

        public MessagesHandler(
            Lifetime lifetime,
            INetworkHolder networkHolder,
            IMessageReceivers messageReceivers)
        {
            _messageReceivers = messageReceivers;
            _networkHolder = networkHolder;

            _networkHolder.NetworkManager.AdviseNotNull(lifetime, network =>
            {
                network.CustomMessagingManager.OnUnnamedMessage += HandleUnnamedMessage;
                lifetime.OnTermination(() => network.CustomMessagingManager.OnUnnamedMessage -= HandleUnnamedMessage);
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

            var netEntries = targetObject.NetEntries
                ?? throw new InvalidOperationException(
                    $"[MessagesHandler] Network object '{header.NetworkObjectId.ToString()}:{netObj.name}' has no net entries, it was instantiated without its DI context.");

            var targetEntry = netEntries.EntryWritableBy(senderClientId, header.VarId);

            _currentReader = reader;
            targetEntry.Accept(this); // it goes directly to VisitProperty or VisitSignal down below
        }

        void INetSyncVisitor.VisitProperty<T>(INetProperty<T> property)
        {
            if (!_messageReceivers.ByType.TryGetValue(typeof(T), out var receiver)) //TODO: нет получателя для типа — тихий выход, кандидат на исключение
                return;

            ((IMessageReceiver<T>)receiver).ReceiveProperty(ref _currentReader, property);
        }

        void INetSyncVisitor.VisitSignal<T>(INetSignal<T> entry)
        {
            if (!_messageReceivers.ByType.TryGetValue(typeof(T), out var receiver)) //TODO: нет получателя для типа — тихий выход, кандидат на исключение
                return;

            ((IMessageReceiver<T>)receiver).ReceiveSignal(ref _currentReader, entry);
        }
    }
}
