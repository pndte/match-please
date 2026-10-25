using System;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Entities.Network.Serialization
{
    public static class BufferSerializerQuantizedExtensions
    {
        private const float FullTurn = 360f;
        private const float TurnSteps = 65536f;
        private const float ShareSteps = ushort.MaxValue;

        public static void SerializeQuantized<T>(this BufferSerializer<T> serializer, ref float value, Quantization quantization)
            where T : IReaderWriter
        {
            var steps = serializer.IsWriter ? quantization.StepsOf(value) : 0;
            serializer.SerializePacked(ref steps);
            value = quantization.ValueOf(steps);
        }

        public static void SerializeQuantized<T>(this BufferSerializer<T> serializer, ref double value, Quantization quantization)
            where T : IReaderWriter
        {
            var steps = serializer.IsWriter ? quantization.StepsOf(value) : 0L;
            serializer.SerializePacked(ref steps);
            value = quantization.ValueOf(steps);
        }

        public static void SerializeQuantized<T>(this BufferSerializer<T> serializer, ref Vector2 value, Quantization quantization)
            where T : IReaderWriter
        {
            var x = value.x;
            var y = value.y;
            serializer.SerializeQuantized(ref x, quantization);
            serializer.SerializeQuantized(ref y, quantization);
            value = new Vector2(x, y);
        }

        public static void SerializeAngle<T>(this BufferSerializer<T> serializer, ref float degrees) where T : IReaderWriter
        {
            var steps = serializer.IsWriter ? TurnStepsOf(degrees) : (ushort)0;
            serializer.SerializeValue(ref steps);
            degrees = Mathf.DeltaAngle(0f, steps * (FullTurn / TurnSteps));
        }

        public static void SerializeShare<T>(this BufferSerializer<T> serializer, ref float share) where T : IReaderWriter
        {
            var steps = serializer.IsWriter ? ShareStepsOf(share) : (ushort)0;
            serializer.SerializeValue(ref steps);
            share = steps / ShareSteps;
        }

        private static ushort TurnStepsOf(float degrees) =>
            unchecked((ushort)checked((int)Math.Round(Mathf.Repeat(degrees, FullTurn) * (TurnSteps / FullTurn))));

        private static ushort ShareStepsOf(float share) =>
            checked((ushort)Math.Round(share * ShareSteps));
    }
}
