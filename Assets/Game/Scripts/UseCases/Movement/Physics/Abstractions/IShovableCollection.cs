using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IShovableCollection : IViewableMap<Collider2D, IShovable>
    {
    }
}
