using Unity.Netcode;

namespace Bw.Entities.Network.Codecs
{
    public struct FloatCodec : ICodec<float>
    {
        public float Value
        {
            get => _value;
            set => _value = value;
        }

        private float _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _value);
        }
    }

    public struct IntCodec : ICodec<int>
    {
        public int Value
        {
            get => _value;
            set => _value = value;
        }

        private int _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _value);
        }
    }

    public struct BoolCodec : ICodec<bool>
    {
        public bool Value
        {
            get => _value;
            set => _value = value;
        }

        private bool _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _value);
        }
    }
}