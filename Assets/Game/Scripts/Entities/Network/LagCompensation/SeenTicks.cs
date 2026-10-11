namespace Bw.Entities.Network.LagCompensation
{
    public readonly struct SeenTicks
    {
        public readonly double Interpolated;
        public readonly double Predicted;

        public SeenTicks(double interpolated, double predicted)
        {
            Interpolated = interpolated;
            Predicted = predicted;
        }
    }
}
