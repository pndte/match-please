using Bw.Entities.Network.Variables;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public interface IReloadRequestResult
    {
        public INetSignal<ReloadRequestResultDto> Received { get; }
    }
}
