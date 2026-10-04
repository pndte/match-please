using Bw.Entities.Network.Variables;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public interface IReloadRequest
    {
        public INetSignal<ReloadRequestDto> Requested { get; }
    }
}