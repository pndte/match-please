using JetBrains.Collections.Viewable;
using JetBrains.Core;

namespace Bw.Entities.Pool
{
    public interface IOneShot
    {
        public ISource<Unit> Finished { get; }
    }
}
