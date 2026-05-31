using System;
using Bw.Entities.Loop;
using Bw.Entities.Network.Repository;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using Bw.UseCases.Shooting.Weapon.Network.Requests;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public class ShootingWeapon : IWeapon, IUpdatable //TODO: static installer
    {
        public ISource<Vector3> OnShot => _onShot;
        public IReadonlyProperty<bool> CanShoot => _canShoot;

        private readonly ISignal<Vector3> _onShot;
        private readonly IReadonlyAmmo _ammo;
        private readonly IReloader _reloader;
        private readonly ShootingWeaponConfig _config;
        private readonly ViewableProperty<bool> _canShoot;

        private float _elapsedTime;
        private float _lastUpdateTime;

        public ShootingWeapon(
            IReadonlyAmmo ammo,
            IReloader reloader,
            ShootingWeaponConfig config)
        {
            _ammo = ammo;
            _reloader = reloader;
            _config = config;
            
            _onShot = new Signal<Vector3>();
            _canShoot = new ViewableProperty<bool>(true);

            _elapsedTime = config.ShootCooldown;
            _lastUpdateTime = Time.time;
        }

        public void Shoot(Vector3 mousePos)
        {
            if (!_canShoot.Value)
                throw new InvalidOperationException("Trying to shoot when not ready (check _canShoot)");

            _canShoot.Value = false;
            _onShot.Fire(mousePos);

            _elapsedTime = _config.ShootCooldown;
        }

        public void Update()
        {
            var currentTime = Time.time;
            var deltaTime = currentTime - _lastUpdateTime;
            _lastUpdateTime = currentTime;

            if (_canShoot.Value || _ammo.Empty() || _reloader.State.Value == ReloadState.Reloading) return;

            Debug.Log("Ammo count: " + _ammo.Value);
            _elapsedTime -= deltaTime;

            if (_elapsedTime <= 0)
            {
                _canShoot.Value = true;
            }
        }

        public class NetworkHandler
        {
            public NetworkHandler(
                Lifetime lifetime,
                ShootingWeapon shootingWeapon,
                IReceivedShot receivedShot,
                IRequestIdsRepository requestIdsRepository)
            {
                receivedShot.Received.Advise(lifetime, shootRequestDto =>
                {
                    if (requestIdsRepository.TryRemoveIdFor<ShootRequestDto>(shootRequestDto.RequestId))
                        return;
                    
                    shootingWeapon._onShot.Fire(shootRequestDto.TargetPosition);
                });
            }
        }
    }
}