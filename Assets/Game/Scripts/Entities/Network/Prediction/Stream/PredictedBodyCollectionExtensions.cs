namespace Bw.Entities.Network.Prediction.Stream
{
    public static class PredictedBodyCollectionExtensions
    {
        public static void PoseFor(this IPredictedBodyCollection bodies, int tick)
        {
            for (var index = 0; index < bodies.Count; index++)
                bodies[index].PoseFor(tick);
        }
    }
}
