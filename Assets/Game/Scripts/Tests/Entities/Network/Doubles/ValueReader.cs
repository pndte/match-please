using Bw.Entities.Network.Variables;

namespace Bw.Tests.Entities.Network.Doubles
{
    internal sealed class ValueReader<TValue> : INetSyncVisitor
    {
        private readonly TValue _value;
        private readonly ulong _senderClientId;

        public ValueReader(TValue value, ulong senderClientId)
        {
            _value = value;
            _senderClientId = senderClientId;
        }

        void INetSyncVisitor.VisitProperty<T>(INetProperty<T> property) =>
            property.ApplyFromNetwork((T)(object)_value);

        void INetSyncVisitor.VisitSignal<T>(INetSignal<T> signal) =>
            signal.ApplyFromNetwork((T)(object)_value, _senderClientId);
    }
}
