using Bw.Entities;
using Bw.Entities.Network.Repository;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public class WeaponRequestsClientHandler  // TODO: norm name, decompose
    {
        private IMouseShootRequest _shootRequest; // TODO: norm name
        private readonly IReloadRequest _reloadRequest;
        private readonly IWeapon _weapon;
        private readonly IReloader _reloader;
        private readonly IChangableCamera _playerCamera;
        private readonly IRequestIdsRepository _requestIdsRepository;

        private SequentialLifetimes _reloadLifetimes;
        
        public WeaponRequestsClientHandler(
            Lifetime lifetime,
            IControlledBy controlledBy,
            IMouseShootRequest shootRequest,
            IReloadRequest reloadRequest,
            IWeapon weapon,
            IReloader reloader,
            IChangableCamera playerCamera,
            IRequestIdsRepository requestIdsRepository)
        {
            _shootRequest = shootRequest;
            _reloadRequest = reloadRequest;
            _weapon = weapon;
            _reloader = reloader;
            _playerCamera = playerCamera;
            _requestIdsRepository = requestIdsRepository;
            controlledBy.Me.WhenTrue(lifetime, WhenControlledByMe);
            _reloadLifetimes = new(lifetime);
        }

        private void WhenControlledByMe(Lifetime lifetime)
        {
            _weapon.CanShoot.WhenTrue(lifetime, canShootLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, canShootLifetime).Subscribe(UpdateShoot)); //TODO: заменга на свою астракцию
            
            Observable.EveryUpdate(UnityFrameProvider.Update, lifetime).Subscribe(UpdateReload);
        }

        private void UpdateShoot(Unit _)
        {
            if (Input.GetMouseButtonDown(0)) //TODO: new input system
            {
                var mousePos = Input.mousePosition;
                var camera = _playerCamera.Current.Value;
                mousePos.z = camera.nearClipPlane;

                var mouseWorldPos = camera.ScreenToWorldPoint(mousePos);

                _shootRequest.Requested.Fire(new ShootRequestDto(_requestIdsRepository.NextIdFor<ShootRequestDto>(), mouseWorldPos));
                _weapon.Shoot(mouseWorldPos);
            }
        }

        private void UpdateReload(Unit _)
        {
            if (Input.GetKeyDown(KeyCode.R)) //TODO: new input system
            {
                _reloadRequest.Requested.Fire();
                _reloader.Reload(_reloadLifetimes.Next()); //TODO: lifetime eternal тут плохо
            }
        }
    }
}