using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public interface IReloadRequestResult
    {
        public ISignal<ReloadRequestResultDto> Received { get; }
    }
}
