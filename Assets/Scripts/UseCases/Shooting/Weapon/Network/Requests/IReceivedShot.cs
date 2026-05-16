using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public interface IReceivedShot
    {
        public ISignal<ShootRequestDto> Received { get; } //TODO: name?
    }
}