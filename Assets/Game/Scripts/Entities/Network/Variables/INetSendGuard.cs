using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    internal interface INetSendGuard
    {
        void Check(NetSendTarget target);
        void WhenOpen(Lifetime lifetime, Action<Lifetime> handler);
    }

    internal sealed class AllowedSendGuard : INetSendGuard
    {
        private readonly bool _canTargetClients;

        public AllowedSendGuard(bool canTargetClients)
        {
            _canTargetClients = canTargetClients;
        }

        public void Check(NetSendTarget target)
        {
            if (target.IsTargeted && !_canTargetClients)
                throw new InvalidOperationException("A client sends only to the server, FireTo is server-only.");
        }

        public void WhenOpen(Lifetime lifetime, Action<Lifetime> handler) =>
            handler(lifetime);
    }

    internal sealed class WhileMineSendGuard : INetSendGuard
    {
        private readonly INetSendGuard _inner;
        private readonly IOwnership _ownership;
        private readonly NetworkObject _networkObject;

        public WhileMineSendGuard(INetSendGuard inner, IOwnership ownership, NetworkObject networkObject)
        {
            _inner = inner;
            _ownership = ownership;
            _networkObject = networkObject;
        }

        public void Check(NetSendTarget target)
        {
            _inner.Check(target);
            if (!_ownership.Mine.Value)
                throw new InvalidOperationException(
                    $"Only the owner sends this entry, object {_networkObject.NetworkObjectId} is not mine now.");
        }

        public void WhenOpen(Lifetime lifetime, Action<Lifetime> handler) =>
            _inner.WhenOpen(lifetime, innerLifetime => _ownership.Mine.WhenTrue(innerLifetime, handler));
    }

    internal sealed class ForbiddenSendGuard : INetSendGuard
    {
        private readonly string _reason;

        public ForbiddenSendGuard(string reason)
        {
            _reason = reason;
        }

        public void Check(NetSendTarget target) =>
            throw new InvalidOperationException(_reason);

        public void WhenOpen(Lifetime lifetime, Action<Lifetime> handler)
        {
        }
    }
}
