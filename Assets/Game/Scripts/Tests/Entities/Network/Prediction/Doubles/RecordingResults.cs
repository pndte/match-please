using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;

namespace Bw.Tests.Entities.Network.Prediction.Doubles
{
    public sealed class RecordingResults<T> : INetResultSender<T>
    {
        public IReadOnlyList<T> Broadcasts => _broadcasts;
        public IReadOnlyList<(IClient Recipient, T Result)> Sent => _sent;

        private readonly List<T> _broadcasts = new();
        private readonly List<(IClient, T)> _sent = new();

        public void Broadcast(T result) =>
            _broadcasts.Add(result);

        public void SendTo(IClient recipient, T result) =>
            _sent.Add((recipient, result));
    }
}
