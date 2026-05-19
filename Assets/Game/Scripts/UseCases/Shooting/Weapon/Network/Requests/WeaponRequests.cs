using JetBrains.Collections.Viewable;
using JetBrains.Core;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public class WeaponRequests : IMouseShootRequest, IReloadRequest, IReceivedShot // TODO: norm name
    {
        public ISignal<ShootRequestDto> Received { get; }
        ISignal<ShootRequestDto> IMouseShootRequest.Requested => _mouseShootRequest;
        ISignal<Unit> IReloadRequest.Requested => _reloadRequest;

        private readonly ISignal<ShootRequestDto> _mouseShootRequest;
        private readonly ISignal<Unit> _reloadRequest;

        public WeaponRequests(ISignal<ShootRequestDto> mouseShootRequest, ISignal<Unit> reloadRequest,
            ISignal<ShootRequestDto> received)
        {
            _mouseShootRequest = mouseShootRequest;
            _reloadRequest = reloadRequest;
            Received = received;
        }
    }
}