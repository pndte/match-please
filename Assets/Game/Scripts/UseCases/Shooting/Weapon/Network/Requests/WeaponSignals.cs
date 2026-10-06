using Bw.Entities.Network.Variables;
using JetBrains.Core;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class WeaponSignals : IWeaponDropRequest //TODO: rename
    {
        INetSignal<Unit> IWeaponDropRequest.Requested => _dropRequest;

        private readonly INetSignal<Unit> _dropRequest;

        public WeaponSignals(INetSignal<Unit> dropRequest)
        {
            _dropRequest = dropRequest;
        }
    }
}
