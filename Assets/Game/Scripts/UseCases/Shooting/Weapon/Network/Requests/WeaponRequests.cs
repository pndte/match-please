using Bw.Entities.Network.Variables;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public class WeaponSignals : IMouseShootRequest, IReloadRequest, IShootRequestResult, IReloadRequestResult
    {
        public INetSignal<ShootRequestResultDto> Received { get; }
        public INetSignal<ReloadRequestResultDto> ReloadReceived { get; }
        INetSignal<ShootRequestDto> IMouseShootRequest.Requested => _mouseShootRequest;
        INetSignal<ReloadRequestDto> IReloadRequest.Requested => _reloadRequest;
        INetSignal<ReloadRequestResultDto> IReloadRequestResult.Received => ReloadReceived;

        private readonly INetSignal<ShootRequestDto> _mouseShootRequest;
        private readonly INetSignal<ReloadRequestDto> _reloadRequest;

        public WeaponSignals(
            INetSignal<ShootRequestDto> mouseShootRequest,
            INetSignal<ReloadRequestDto> reloadRequest,
            INetSignal<ShootRequestResultDto> shootReceived,
            INetSignal<ReloadRequestResultDto> reloadReceived)
        {
            _mouseShootRequest = mouseShootRequest;
            _reloadRequest = reloadRequest;
            Received = shootReceived;
            ReloadReceived = reloadReceived;
        }
    }
}
