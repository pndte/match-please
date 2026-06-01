using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public interface IReloadRequest
    {
        public ISignal<ReloadRequestDto> Requested { get; }
    }
}