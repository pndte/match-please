using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public interface IMouseShootRequest
    {
        public ISignal<ShootRequestDto> Requested { get; }
    }
}