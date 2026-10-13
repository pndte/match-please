using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class ShovableCollection : ViewableMap<Collider2D, IShovable>, IShovableCollection
    {
    }
}
