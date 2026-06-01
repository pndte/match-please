using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public class WeaponSignals : IMouseShootRequest, IReloadRequest, IShootRequestResult, IReloadRequestResult
    {
        public ISignal<ShootRequestResultDto> Received { get; }
        public ISignal<ReloadRequestResultDto> ReloadReceived { get; }
        ISignal<ShootRequestDto> IMouseShootRequest.Requested => _mouseShootRequest;
        ISignal<ReloadRequestDto> IReloadRequest.Requested => _reloadRequest;
        ISignal<ReloadRequestResultDto> IReloadRequestResult.Received => ReloadReceived;

        private readonly ISignal<ShootRequestDto> _mouseShootRequest;
        private readonly ISignal<ReloadRequestDto> _reloadRequest;

        public WeaponSignals(
            ISignal<ShootRequestDto> mouseShootRequest,
            ISignal<ReloadRequestDto> reloadRequest,
            ISignal<ShootRequestResultDto> shootReceived,
            ISignal<ReloadRequestResultDto> reloadReceived)
        {
            _mouseShootRequest = mouseShootRequest;
            _reloadRequest = reloadRequest;
            Received = shootReceived;
            ReloadReceived = reloadReceived;
        }
    }
}
