using JetBrains.Collections.Viewable;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public interface IPredictableCollection : IViewableMap<PhysicsBody, IPredictable>
    {
    }

    public sealed class PredictableCollection : ViewableMap<PhysicsBody, IPredictable>, IPredictableCollection
    {
    }
}
