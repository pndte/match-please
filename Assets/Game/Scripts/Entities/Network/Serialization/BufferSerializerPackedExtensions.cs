using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace Bw.Entities.Network.Serialization
{
    /// <summary>
    /// Variable-length packing via <see cref="BytePacker"/> / <see cref="ByteUnpacker"/>,
    /// same as <see cref="NetworkMessageHeader"/>.
    /// </summary>
    public static class BufferSerializerPackedExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializePacked<T>(this BufferSerializer<T> serializer, ref bool value)
            where T : IReaderWriter
        {
            if (serializer.IsWriter)
                BytePacker.WriteValuePacked(serializer.GetFastBufferWriter(), value);
            else
                ByteUnpacker.ReadValuePacked(serializer.GetFastBufferReader(), out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializePacked<T>(this BufferSerializer<T> serializer, ref byte value)
            where T : IReaderWriter
        {
            if (serializer.IsWriter)
                BytePacker.WriteValuePacked(serializer.GetFastBufferWriter(), value);
            else
                ByteUnpacker.ReadValuePacked(serializer.GetFastBufferReader(), out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializePacked<T>(this BufferSerializer<T> serializer, ref ushort value)
            where T : IReaderWriter
        {
            if (serializer.IsWriter)
                BytePacker.WriteValuePacked(serializer.GetFastBufferWriter(), value);
            else
                ByteUnpacker.ReadValuePacked(serializer.GetFastBufferReader(), out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializePacked<T>(this BufferSerializer<T> serializer, ref uint value)
            where T : IReaderWriter
        {
            if (serializer.IsWriter)
                BytePacker.WriteValuePacked(serializer.GetFastBufferWriter(), value);
            else
                ByteUnpacker.ReadValuePacked(serializer.GetFastBufferReader(), out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializePacked<T>(this BufferSerializer<T> serializer, ref int value)
            where T : IReaderWriter
        {
            if (serializer.IsWriter)
                BytePacker.WriteValuePacked(serializer.GetFastBufferWriter(), value);
            else
                ByteUnpacker.ReadValuePacked(serializer.GetFastBufferReader(), out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SerializePacked<T>(this BufferSerializer<T> serializer, ref ulong value)
            where T : IReaderWriter
        {
            if (serializer.IsWriter)
                BytePacker.WriteValuePacked(serializer.GetFastBufferWriter(), value);
            else
                ByteUnpacker.ReadValuePacked(serializer.GetFastBufferReader(), out value);
        }
    }
}
