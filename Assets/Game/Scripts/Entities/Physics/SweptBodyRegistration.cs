using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Physics
{
    public sealed class SweptBodyRegistration
    {
        public SweptBodyRegistration(Lifetime lifetime, SweptBody body, ISweptBodyCollection bodies)
        {
            bodies.AddLifetimed(lifetime, body);
        }
    }
}
