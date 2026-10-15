using Bw.Entities;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class FlashlightClientHandler //TODO: фонарик ждёт ответа сервера (полный пинг): мгновенным он станет, когда владелец начнёт предсказывать переключение
    {
        private readonly IReadonlyFlashlight _flashlight;
        private readonly INetRequestSender<bool> _switch;

        public FlashlightClientHandler(
            Lifetime lifetime,
            IControlledBy controlledBy,
            IReadonlyFlashlight flashlight,
            INetRequestSender<bool> @switch)
        {
            _flashlight = flashlight;
            _switch = @switch;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(_ => UpdateSwitch()));
        }

        private void UpdateSwitch()
        {
            if (Input.GetKeyDown(KeyCode.F)) //TODO: new input system
                _switch.Send(!_flashlight.Lit.Value);
        }
    }
}
