using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Triggers
{
    public interface IMouseShootTrigger
    {
        public ISignal<Vector3> Triggered { get; }
    }
}