using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Prediction.Bubble
{
    public interface IBubbleMembership
    {
        public IReadonlyProperty<bool> Inside { get; }
    }
}
