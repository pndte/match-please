using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.Entities.Network.Prediction.Events.Codecs
{
    public static class StateCauseSerialization
    {
        public static void SerializeCause<T>(this BufferSerializer<T> serializer, ref StateCause cause) where T : IReaderWriter
        {
            var isAction = cause.IsAction;
            var initiator = cause.Action.Initiator;
            var tick = cause.Action.Tick;

            serializer.SerializePacked(ref isAction);
            if (isAction)
            {
                serializer.SerializePacked(ref initiator);
                serializer.SerializePacked(ref tick);
            }

            cause = isAction ? StateCause.Of(new ActionId(initiator, tick)) : StateCause.None;
        }
    }
}
