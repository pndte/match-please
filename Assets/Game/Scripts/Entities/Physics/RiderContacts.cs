using System.Collections.Generic;
using JetBrains.Lifetimes;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics
{
    public sealed class RiderContacts : PhysicsCallbacks.IPreSolveCallback
    {
        private const float Upward = 0.7f;

        private readonly HashSet<PhysicsShape> _riders = new();

        public void Add(Lifetime lifetime, PhysicsShape rider)
        {
            _riders.Add(rider);
            lifetime.OnTermination(() => _riders.Remove(rider));
        }

        public bool OnPreSolve2D(PhysicsEvents.PreSolveEvent contact)
        {
            if (_riders.Contains(contact.shapeB))
                return contact.normal.y <= Upward;

            if (_riders.Contains(contact.shapeA))
                return -contact.normal.y <= Upward;

            return true;
        }
    }
}
