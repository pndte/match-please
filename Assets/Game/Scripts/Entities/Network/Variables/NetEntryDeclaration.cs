using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public abstract class NetEntryDeclaration
    {
        internal NetworkDelivery DeliveryType { get; }
        internal NetworkPermissions Permissions { get; }

        private protected NetEntryDeclaration(NetworkDelivery deliveryType, NetworkPermissions permissions)
        {
            DeliveryType = deliveryType;
            Permissions = permissions;
        }

        internal abstract INetSyncEntry Create(INetSendGuard sendGuard);
    }

    public sealed class NetSignalDeclaration<T> : NetEntryDeclaration
    {
        internal NetSignalDeclaration(NetworkDelivery deliveryType, NetworkPermissions permissions)
            : base(deliveryType, permissions)
        {
        }

        internal override INetSyncEntry Create(INetSendGuard sendGuard) =>
            new NetSignal<T>(sendGuard);
    }

    public sealed class NetPropertyDeclaration<T> : NetEntryDeclaration
    {
        private readonly T _initial;

        internal NetPropertyDeclaration(T initial, NetworkDelivery deliveryType, NetworkPermissions permissions)
            : base(deliveryType, permissions)
        {
            _initial = initial;
        }

        internal override INetSyncEntry Create(INetSendGuard sendGuard) =>
            new NetProperty<T>(_initial, sendGuard);
    }
}
