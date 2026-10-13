using Bw.Entities;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class WeaponSwitchClientHandler //TODO: смена оружия ждёт ответа сервера (полный пинг): мгновенной она станет, когда выбор слота войдёт в предсказываемую симуляцию
    {
        private readonly WeaponInventoryConfig _config;
        private readonly INetRequestSender<WeaponSwitch> _switch;

        public WeaponSwitchClientHandler(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            WeaponInventoryConfig config,
            INetRequestSender<WeaponSwitch> @switch)
        {
            _config = config;
            _switch = @switch;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(_ => UpdateSwitch()));
        }

        private void UpdateSwitch() //TODO: new input system
        {
            for (var index = 0; index < _config.Slots.Length; index++)
                if (Input.GetKeyDown(_config.Slots[index].Key))
                    _switch.Send(WeaponSwitch.ToSlot(index));

            if (Input.GetKeyDown(_config.LastKey))
                _switch.Send(WeaponSwitch.ToLast);

            var scroll = Input.mouseScrollDelta.y;
            if (scroll != 0f)
                _switch.Send(WeaponSwitch.Scrolled(scroll > 0f ? -1 : 1));
        }
    }
}
