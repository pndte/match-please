using Unity.Netcode;

namespace Bw.UseCases.Movement.Network.Codecs
{
    public static class BodyStateSerialization
    {
        public static void SerializeBody<T>(this BufferSerializer<T> serializer, ref BodyState body) where T : IReaderWriter
        {
            var position = body.Position;
            var velocity = body.Velocity;
            var horizontalPush = body.HorizontalPush;
            var pushed = horizontalPush != 0f;
            var grounded = body.Grounded;

            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref pushed);
            if (pushed)
                serializer.SerializeValue(ref horizontalPush);

            serializer.SerializeValue(ref grounded);

            body = new BodyState(position, velocity, horizontalPush, grounded);
        }
    }
}
