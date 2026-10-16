using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Movement.Network.Codecs
{
    public static class RigidStateSerialization
    {
        private static readonly Quantization Position = new(8);
        private static readonly Quantization Velocity = new(6);
        private static readonly Quantization Spin = new(4);

        public static void SerializeRigid<T>(this BufferSerializer<T> serializer, ref RigidState state) where T : IReaderWriter
        {
            var position = state.Position;
            var angle = state.Angle;
            var velocity = state.Velocity;
            var angularVelocity = state.AngularVelocity;

            serializer.SerializeQuantized(ref position, Position);
            serializer.SerializeAngle(ref angle);
            serializer.SerializeQuantized(ref velocity, Velocity);
            serializer.SerializeQuantized(ref angularVelocity, Spin);

            state = new RigidState(position, angle, velocity, angularVelocity);
        }
    }
}
