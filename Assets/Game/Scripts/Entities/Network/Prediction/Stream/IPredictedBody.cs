namespace Bw.Entities.Network.Prediction.Stream
{
    public interface IPredictedBody
    {
        public void PoseFor(int tick);
    }
}
