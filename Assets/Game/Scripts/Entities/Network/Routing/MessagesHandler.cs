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
                    $"[MessagesHandler] No spawned network object with id '{header.NetworkObjectId}' " +
                    $"(varId={header.VarId}, sender={senderClientId}). Message ignored.");
                return;
            }

            if (!netObj.TryGetComponent<INetworkLifetimedObject>(out var targetObject))
            {
                throw new Exception(
                    $"[MessagesHandler] Network object '{header.NetworkObjectId}' has no {nameof(INetworkLifetimedObject)}. Message ignored.");
            }

            var variablesTable = targetObject.NetVariablesTable;
            if (!variablesTable.PropertiesByIndex.TryGetValue(header.VarId, out var targetEntry))
            {
                throw new Exception(
                    $"[MessagesHandler] No network property with id '{header.VarId}' on object '{header.NetworkObjectId}'. Message ignored.");
            }

            _currentReader = reader;
            targetEntry.Accept(this);
        }

        public void VisitProperty<T>(INetProperty<T> property)
        {
            if (!_messageReceivers.ByType.TryGetValue(typeof(T), out var receiver))
                return;

            ((IMessageReceiver<T>)receiver).ReceiveProperty(ref _currentReader, property);
        }

        public void VisitSignal<T>(INetSignal<T> entry)
        {
            if (!_messageReceivers.ByType.TryGetValue(typeof(T), out var receiver))
                return;

            ((IMessageReceiver<T>)receiver).ReceiveSignal(ref _currentReader, entry);
        }
    }
}
