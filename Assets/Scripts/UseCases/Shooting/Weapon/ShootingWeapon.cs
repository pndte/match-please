using System;
using Bw.Entities.Loop;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;
using UnityEngine;
using Bw.UseCases.Shooting.Weapon.Extensions;
using Bw.UseCases.Shooting.Weapon.Triggers;
using Cysharp.Threading.Tasks;

namespace Bw.UseCases.Shooting.Weapon
{
    public class ShootingWeapon : IWeapon, IReloader, IUpdatable //TODO: decompose
    {
        public ISource<Vector3> Shot => _shotSignal;
        public IReadonlyProperty<bool> ReadyToShot => _readyToShot;
        public IReadonlyProperty<bool> Reloading => _reloading;
        public ISource<Unit> ReloadComplete => _reloadComplete;
        
        private readonly Signal<Vector3> _shotSignal;
        private readonly ISignal<Unit> _reloadComplete;
        private readonly IMouseShootTrigger _iMouseShootTrigger;
        private readonly IReadonlyAmmo _ammo;
        private readonly ShootingWeaponConfig _config;
        private readonly ViewableProperty<bool> _readyToShot;
        private readonly ViewableProperty<bool> _reloading;
        
        private float _elapsedTime;
        private float _lastUpdateTime;

        public ShootingWeapon(
            Lifetime lifetime, 
            IMouseShootTrigger iMouseShootTrigger,
            IReloadTrigger reloadTrigger,
            IReadonlyAmmo ammo,
            ShootingWeaponConfig config)
        {
            _iMouseShootTrigger = iMouseShootTrigger;
            _ammo = ammo;
            _config = config;
            _shotSignal = new Signal<Vector3>();
            _readyToShot = new ViewableProperty<bool>(true);
            _reloading = new ViewableProperty<bool>(false);
            _reloadComplete = new Signal<Unit>();
            _elapsedTime = config.ShootCooldown;
            _lastUpdateTime = Time.time;
            
            _readyToShot.WhenTrue(lifetime, WaitForTrigger);
            _ammo.WhenEmpty(lifetime, lf => Reload(lf).Forget());
            reloadTrigger.Triggered.Advise(lifetime, triggerLifetime =>
                _ammo.View(triggerLifetime, (ammoLifetime, _) =>
                    Reload(ammoLifetime).Forget()));
        }
        
        public void ForceShot(Vector3 targetPosition)
        {
            _shotSignal.Fire(targetPosition);
        }

        public void Update()
        {
            var currentTime = Time.time;
            var deltaTime = currentTime - _lastUpdateTime;
            _lastUpdateTime = currentTime;
            
            if (_readyToShot.Value || _ammo.Empty() || _reloading.Value) return;
            
            _elapsedTime -= deltaTime;

            if (_elapsedTime <= 0)
            {
                _readyToShot.Value = true;
            }
        }

        public async UniTaskVoid Reload(Lifetime lifetime)
        {
            _reloading.Value = true;
            lifetime.OnTermination(() => _reloading.Value = false);
            
            var cancelled = await UniTask.Delay(TimeSpan.FromSeconds(_config.ReloadTime), cancellationToken: lifetime).SuppressCancellationThrow();
            if (cancelled) return;
            
            _reloading.Value = false;
            _reloadComplete.Fire();
        }
        
        private void WaitForTrigger(Lifetime lifetime)
        {
            _iMouseShootTrigger.Triggered.Advise(lifetime, Shoot);
        }

        private void Shoot(Vector3 mousePos)
        {
            _readyToShot.Value = false;
            _shotSignal.Fire(mousePos);
            
            _elapsedTime = _config.ShootCooldown;
        }
    }
}