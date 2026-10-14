using Bw.Entities.Network;
using Bw.Entities.Network.Serialization;
using Bw.UseCases.Shooting.Weapon.Network.Requests;
using Unity.Netcode;

namespace Bw.UseCases.Shooting.Weapon.Network.Codecs
{
    public struct WeaponPickupCodec : ICodec<WeaponPickup>
    {
        private static readonly Quantization Point = new(4);

        public WeaponPickup Value
        {
            get => _value;
            set => _value = value;
        }

        private WeaponPickup _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var point = Value.Point;
            serializer.SerializeQuantized(ref point, Point);
            Value = new WeaponPickup(point);
        }
    }
}
