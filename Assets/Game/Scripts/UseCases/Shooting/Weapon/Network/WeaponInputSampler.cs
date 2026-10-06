using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class WeaponInputSampler : IInputSampler<WeaponInput>
    {
        private const float MinAimDistance = 0.01f;

        private readonly Transform _weapon;
        private readonly IChangableCamera _camera;
        private readonly INetworkTicks _ticks;
        private readonly IInterpolationTicks _interpolationTicks;

        private bool _held;
        private float _aim;
        private bool _triggerPressed;
        private double _triggerViewTick;
        private bool _reloadPressed;

        public WeaponInputSampler(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IWeaponHold hold,
            IChangableCamera camera,
            INetworkTicks ticks,
            IInterpolationTicks interpolationTicks,
            Transform weapon)
        {
            _weapon = weapon;
            _camera = camera;
            _ticks = ticks;
            _interpolationTicks = interpolationTicks;

            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                _held = true;
                heldLifetime.OnTermination(() => _held = false);
            });
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(_ => LatchPresses()));
        }

        public WeaponInput Sample() //TODO: нажатие курка теряется, если пропадут два пакета ввода подряд (Unreliable и один прошлый ввод в пакете) — для стрельбы нужна избыточность больше
        {
            var viewDelay = _triggerPressed ? (float)(_ticks.Current - _triggerViewTick) : 0f;
            var input = new WeaponInput(SampleAim(), _triggerPressed, _reloadPressed, viewDelay);
            _triggerPressed = false;
            _reloadPressed = false;
            return input;
        }

        private void LatchPresses()
        {
            if (Input.GetMouseButtonDown(0) && !_triggerPressed) //TODO: new input system
            {
                _triggerPressed = true;
                _triggerViewTick = _interpolationTicks.InterpolationTick;
            }

            if (Input.GetKeyDown(KeyCode.R)) //TODO: new input system
                _reloadPressed = true;
        }

        private float SampleAim()
        {
            if (!_held)
                return _aim;

            var camera = _camera.Current.Value;
            var mouse = Input.mousePosition; //TODO: new input system
            mouse.z = camera.nearClipPlane;

            var target = (Vector2)_weapon.parent.InverseTransformPoint(camera.ScreenToWorldPoint(mouse));
            if (target.sqrMagnitude > MinAimDistance * MinAimDistance)
                _aim = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;

            return _aim;
        }
    }
}
