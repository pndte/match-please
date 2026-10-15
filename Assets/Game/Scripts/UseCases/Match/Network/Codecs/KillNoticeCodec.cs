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
            var killer = Value.Killer.ClientId;
            var killerName = serializer.IsWriter ? Value.Killer.Nickname : string.Empty;
            var victim = Value.Victim.ClientId;
            var victimName = serializer.IsWriter ? Value.Victim.Nickname : string.Empty;
            var weapon = Value.WeaponKey;

            serializer.SerializePacked(ref killer);
            serializer.SerializeValue(ref killerName);
            serializer.SerializePacked(ref victim);
            serializer.SerializeValue(ref victimName);
            serializer.SerializePacked(ref weapon);

            if (serializer.IsWriter)
                return;

            Value = new KillNotice(new PlayerTag(killer, killerName), new PlayerTag(victim, victimName), weapon);
        }
    }
}
