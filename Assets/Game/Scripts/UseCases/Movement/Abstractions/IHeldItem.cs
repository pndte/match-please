using JetBrains.Collections.Viewable;
using JetBrains.Core;
using UnityEngine;

namespace Bw.UseCases.Movement.Abstractions
{
    public interface IHeldItem
    {
        public ISource<Unit> Turned { get; }
        public void PlaceIn(Rect oval);
    }
}
