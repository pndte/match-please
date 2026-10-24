using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Rollback
{
    public interface IRollbackParts
    {
        public void Track(Lifetime lifetime, IRollbackPart part);
    }
}
