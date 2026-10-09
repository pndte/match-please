namespace Bw.Entities.Network.Variables
{
    internal interface INetSignal<T> : INetSyncEntry
    {
        T PendingPayload { get; }
        NetSendTarget PendingTarget { get; }
        void ApplyFromNetwork(T value);
    }
}
