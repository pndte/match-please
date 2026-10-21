using Bw.Entities.Network;
using Bw.Entities.Network.Serialization;
using Bw.UseCases.Match.Network.Requests;
using Unity.Netcode;

namespace Bw.UseCases.Match.Network.Codecs
{
    public struct KillNoticeCodec : ICodec<KillNotice>
    {
        public KillNotice Value
        {
            get => _value;
            set => _value = value;
        }

        private KillNotice _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var killer = Value.Killer;
            var victim = Value.Victim;
            var weapon = Value.WeaponKey;

            serializer.SerializeTag(ref killer);
            serializer.SerializeTag(ref victim);
            serializer.SerializePacked(ref weapon);

            if (serializer.IsWriter)
                return;

            Value = new KillNotice(killer, victim, weapon);
        }
    }
}
