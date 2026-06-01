using Bw.Entities;
using Bw.Entities.Network.Repository;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Cysharp.Threading.Tasks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public class WeaponRequestsClientHandler // TODO: norm name, decompose
    {
        private IMouseShootRequest _shootRequest; // TODO: norm name
        private readonly IReloadRequest _reloadRequest;
        private readonly IWeapon _weapon;
        private readonly IReloader _reloader;
        private readonly IChangableCamera _playerCamera;
        private readonly IRequestIdsRepository _requestIdsRepository;
        private readonly PendingReloadLifetimes _pendingReloadLifetimes;

        private Lifetime _reloadScope;

        public WeaponRequestsClientHandler(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IMouseShootRequest shootRequest,
            IReloadRequest reloadRequest,
            IWeapon weapon,
            IReloader reloader,
            IChangableCamera playerCamera,
            IRequestIdsRepository requestIdsRepository,
            PendingReloadLifetimes pendingReloadLifetimes)
        {
            _shootRequest = shootRequest;
            _reloadRequest = reloadRequest;
            _weapon = weapon;
            _reloader = reloader;
            _playerCamera = playerCamera;
            _requestIdsRepository = requestIdsRepository;
            _pendingReloadLifetimes = pendingReloadLifetimes;
            controlledBy.Me.WhenTrue(lifetime, WhenControlledByMe);
        }

        private void WhenControlledByMe(Lifetime lifetime)
        {
            _reloadScope = lifetime;

            _weapon.CanShoot.WhenTrue(lifetime, canShootLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, canShootLifetime).Subscribe(UpdateShoot)); //TODO: заменить на свою абстракцию

            _reloader.CanReload.WhenTrue(lifetime, canReloadLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, canReloadLifetime).Subscribe(UpdateReload));
        }

        private void UpdateShoot(Unit _)
        {
            //TODO: new input system
            if (Input.GetMouseButtonDown(0)
                && _reloader.State.Value != ReloadState.Reloading
                && _weapon.CanShoot.Value)
            {
                var mousePos = Input.mousePosition;
                var camera = _playerCamera.Current.Value;
                mousePos.z = camera.nearClipPlane;

                var mouseWorldPos = camera.ScreenToWorldPoint(mousePos);

                _shootRequest.Requested.Fire(new ShootRequestDto(
                    _requestIdsRepository.NextIdFor<ShootRequestDto>(), mouseWorldPos));
                _weapon.Shoot(mouseWorldPos);
            }
        }

        private void UpdateReload(Unit _)
        {
            if (Input.GetKeyDown(KeyCode.R)) //TODO: new input system
            {
                var requestId = _requestIdsRepository.NextIdFor<ReloadRequestDto>();
                var reloadDefinition = _reloadScope.CreateNested();
                _pendingReloadLifetimes.Register(requestId, reloadDefinition);

                _reloadRequest.Requested.Fire(new ReloadRequestDto(requestId));
                _reloader.Reload(reloadDefinition.Lifetime).Forget(); //TODO: lifetime policy при отмене/повторной перезарядке
            }
        }
    }
}
