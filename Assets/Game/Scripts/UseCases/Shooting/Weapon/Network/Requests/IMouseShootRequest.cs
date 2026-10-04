using Bw.Entities.Network.Variables;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public interface IMouseShootRequest
    {
        public INetSignal<ShootRequestDto> Requested { get; }
    }
}