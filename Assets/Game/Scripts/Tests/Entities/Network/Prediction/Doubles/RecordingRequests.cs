using System.Collections.Generic;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.Entities.Network.Prediction.Doubles
{
    public sealed class RecordingRequests<T> : INetRequestSender<T>
    {
        public ISource<T> Sent => _sent;
        public IReadOnlyList<T> Requests => _requests;

        private readonly Signal<T> _sent = new();
        private readonly List<T> _requests = new();

        public void Send(T request)
        {
            _requests.Add(request);
            _sent.Fire(request);
        }
    }
}
