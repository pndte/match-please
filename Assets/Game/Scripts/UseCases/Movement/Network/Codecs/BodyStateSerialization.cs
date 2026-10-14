using Bw.Entities.Network.Serialization;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Codecs
{
    public static class BodyStateSerialization
    {
        private static readonly Quantization Position = new(8);
        private static readonly Quantization Velocity = new(6);

        public static void SerializeMotion<T>(
            this BufferSerializer<T> serializer,
            ref Vector2 position,
            ref Vector2 velocity,
            ref float horizontalPush) where T : IReaderWriter
        {
            serializer.SerializeQuantized(ref position, Position);
            serializer.SerializeQuantized(ref velocity, Velocity);
            serializer.SerializeQuantized(ref horizontalPush, Velocity);
        }
    }
}
