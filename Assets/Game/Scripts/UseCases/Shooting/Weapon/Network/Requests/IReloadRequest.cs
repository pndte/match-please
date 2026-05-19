using JetBrains.Collections.Viewable;
using JetBrains.Core;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public interface IReloadRequest
    {
        public ISignal<Unit> Requested { get; }
    }
}