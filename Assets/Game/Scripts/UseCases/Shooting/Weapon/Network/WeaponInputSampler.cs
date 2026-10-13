using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Stream;
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
        private readonly TriggerLatch _trigger = new();

        private bool _drawn;
        private float _aim;
        private double _triggerViewTick;
        private double _triggerPredictedTick;
        private bool _reloadPressed;

        public WeaponInputSampler(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IReadonlyWeaponDraw draw,
            IChangableCamera camera,
            INetworkTicks ticks,
            IInterpolationTicks interpolationTicks,
            Transform weapon)
        {
            _weapon = weapon;
            _camera = camera;
            _ticks = ticks;
            _interpolationTicks = interpolationTicks;

            draw.DrawnLifetime.WhenAlive(lifetime, drawnLifetime =>
            {
                _drawn = true;
                drawnLifetime.OnTermination(() => _drawn = false);
            });
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
            {
                _trigger.Begin(Input.GetMouseButton(0)); //TODO: new input system
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(_ => LatchPresses());
                controlledLifetime.OnTermination(ForgetPresses);
            });
        }

        public WeaponInput Sample() //TODO: нажатие курка теряется, если пропадут два пакета ввода подряд (Unreliable и один прошлый ввод в пакете) — для стрельбы нужна избыточность больше
        {
            var trigger = _trigger.Pulled(Input.GetMouseButton(0)); //TODO: new input system
            var viewTick = _trigger.Latched ? _triggerViewTick : _interpolationTicks.InterpolationTick;
            var predictedTick = _trigger.Latched ? _triggerPredictedTick : _ticks.PredictedViewTick(_interpolationTicks);
            var viewDelay = trigger ? (float)(_ticks.Current - viewTick) : 0f;
            var predictedDelay = trigger ? (float)(_ticks.Current - predictedTick) : 0f;
            var input = new WeaponInput(SampleAim(), trigger, _reloadPressed, viewDelay, predictedDelay);
            _trigger.Clear();
            _reloadPressed = false;
            return input;
        }

        private void LatchPresses()
        {
            if (Input.GetMouseButtonDown(0) && !_trigger.Latched) //TODO: new input system
            {
                _trigger.Latch();
                _triggerViewTick = _interpolationTicks.InterpolationTick;
                _triggerPredictedTick = _ticks.PredictedViewTick(_interpolationTicks);
            }

            if (Input.GetKeyDown(KeyCode.R)) //TODO: new input system
                _reloadPressed = true;
        }

        private void ForgetPresses()
        {
            _trigger.Clear();
            _reloadPressed = false;
        }

        private float SampleAim()
        {
            if (!_drawn)
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
