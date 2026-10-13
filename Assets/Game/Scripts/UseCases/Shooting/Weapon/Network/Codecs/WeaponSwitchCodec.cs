using Bw.Entities.Network;
using Bw.UseCases.Shooting.Weapon.Network.Requests;
using Unity.Netcode;

namespace Bw.UseCases.Shooting.Weapon.Network.Codecs
{
    public struct WeaponSwitchCodec : ICodec<WeaponSwitch>
    {
        public WeaponSwitch Value
        {
            get => _value;
            set => _value = value;
        }

        private WeaponSwitch _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var kind = (byte)Value.Kind;
            var value = Value.Value;
            serializer.SerializeValue(ref kind);
            serializer.SerializeValue(ref value);
            Value = new WeaponSwitch((WeaponSwitchKind)kind, value);
        }
    }
}
