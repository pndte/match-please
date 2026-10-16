using System.Collections.Generic;
using JetBrains.Lifetimes;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics
{
    public sealed class OneWayContacts : PhysicsCallbacks.IPreSolveCallback
    {
        private const float Upward = 0.7f;

        private readonly HashSet<PhysicsShape> _planks = new();

        public void Add(Lifetime lifetime, PhysicsShape plank)
        {
            _planks.Add(plank);
            lifetime.OnTermination(() => _planks.Remove(plank));
        }

        public bool OnPreSolve2D(PhysicsEvents.PreSolveEvent contact)
        {
            if (_planks.Contains(contact.shapeA))
                return contact.normal.y > Upward;

            if (_planks.Contains(contact.shapeB))
                return -contact.normal.y > Upward;

            return true;
        }
    }
}
