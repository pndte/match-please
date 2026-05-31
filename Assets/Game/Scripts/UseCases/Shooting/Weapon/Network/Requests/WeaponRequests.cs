using JetBrains.Collections.Viewable;
using JetBrains.Core;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public class WeaponSignals : IMouseShootRequest, IReloadRequest, IShootRequestResult
    {
        public ISignal<ShootRequestDto> Received { get; }
        ISignal<ShootRequestDto> IMouseShootRequest.Requested => _mouseShootRequest;
        ISignal<Unit> IReloadRequest.Requested => _reloadRequest;

        private readonly ISignal<ShootRequestDto> _mouseShootRequest;
        private readonly ISignal<Unit> _reloadRequest;

        public WeaponSignals(ISignal<ShootRequestDto> mouseShootRequest, ISignal<Unit> reloadRequest,
            ISignal<ShootRequestDto> received)
        {
            _mouseShootRequest = mouseShootRequest;
            _reloadRequest = reloadRequest;
            Received = received;
        }
    }
}