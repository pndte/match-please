using Bw.Entities.Network.Variables;
using JetBrains.Core;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public interface IWeaponDropRequest
    {
        public INetSignal<Unit> Requested { get; }
    }
}
