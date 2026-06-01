using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public interface IShootRequestResult
    {
        public ISignal<ShootRequestResultDto> Received { get; } //TODO: name?
    }
}