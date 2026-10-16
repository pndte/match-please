using System;
using Bw.Entities;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Mortality.Extensions
{
    public static class PhysicsShapeMortalExtensions
    {
        public static void SetMortal(this PhysicsShape shape, IHolder<IReadonlyMortal> mortal)
        {
            if (mortal is not UnityEngine.Object holder)
                throw new ArgumentException($"A shape links to its mortal through the user data, which holds only Unity objects: {mortal.GetType().Name} is not one.", nameof(mortal));

            var data = shape.userData;
            data.objectValue = holder;
            shape.userData = data;
        }

        public static bool TryGetMortal(this PhysicsShape shape, out IReadonlyMortal mortal)
        {
            if (shape.userData.objectValue is IHolder<IReadonlyMortal> holder)
            {
                mortal = holder.Value;
                return true;
            }

            mortal = default;
            return false;
        }
    }
}
