using System;
using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class RecordingResults<T> : INetResultSender<T>
    {
        public IReadOnlyList<T> Broadcasts => _broadcasts;

        private readonly List<T> _broadcasts = new();

        public void Broadcast(T result) =>
            _broadcasts.Add(result);

        public void SendTo(IClient recipient, T result) =>
            throw new NotSupportedException("The recording results only take broadcasts.");
    }
}
