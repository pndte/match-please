using Bw.Entities.Network.Variables;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public interface IShootRequestResult
    {
        public INetSignal<ShootRequestResultDto> Received { get; } //TODO: name?
    }
}