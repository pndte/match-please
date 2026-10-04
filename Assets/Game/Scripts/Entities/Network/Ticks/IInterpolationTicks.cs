namespace Bw.Entities.Network.Ticks
{
    public interface IInterpolationTicks
    {
        public float Progress { get; }
        public double InterpolationTick { get; }
    }
}
