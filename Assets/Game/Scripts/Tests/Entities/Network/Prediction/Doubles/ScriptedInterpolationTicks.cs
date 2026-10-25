using Bw.Entities.Network.Ticks;

namespace Bw.Tests.Entities.Network.Prediction.Doubles
{
    public sealed class ScriptedInterpolationTicks : IInterpolationTicks
    {
        public float Progress { get; set; }
        public double InterpolationTick { get; set; }
    }
}
