using Bw.Entities;
using Bw.Entities.Loop;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Triggers
{
    public class KeyboardWeaponTriggersConnector : IUpdatable
    {
        private readonly IWeapon _weapon;
        private readonly IReloadTrigger _reloadTrigger;
        private readonly IMouseShootTrigger _mouseShootTrigger;
        private readonly IChangableCamera _playerCamera;
        private readonly SequentialLifetimes _reloadLifetimes;

        public KeyboardWeaponTriggersConnector(
            Lifetime lifetime, 
            IWeapon weapon, 
            IReloadTrigger reloadTrigger, 
            IMouseShootTrigger mouseShootTrigger,
            IChangableCamera playerCamera)
        {
            _weapon = weapon;
            _reloadTrigger = reloadTrigger;
            _mouseShootTrigger = mouseShootTrigger;
            _playerCamera = playerCamera;
            _reloadLifetimes = new SequentialLifetimes(lifetime);
        }

        public void Update() // TODO: это место должно вызываться только owner-ом. Настроить это можно будет в стороннем скрипте
        {
            if (Input.GetMouseButtonDown(0) && _weapon.ReadyToShot.Value) // TODO: заменить на новую инпут систему
            {
                var mousePos = Input.mousePosition;
                var camera = _playerCamera.Current.Value;
                mousePos.z = camera.nearClipPlane;
                
                var mouseWorldPos = camera.ScreenToWorldPoint(mousePos);
                _mouseShootTrigger.Triggered.Fire(mouseWorldPos);
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                _reloadTrigger.Triggered.Fire(_reloadLifetimes.Next());
            }
        }
    }
}