namespace Bw.Entities.Network.Ticks
{
    public static class NetworkTicksExtensions
    {
        public static double PredictedViewTick(this INetworkTicks ticks, IInterpolationTicks interpolation) =>
            ticks.Current - 1 + (double)interpolation.Progress;
    }
}
