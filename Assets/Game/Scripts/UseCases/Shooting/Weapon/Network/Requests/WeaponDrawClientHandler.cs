using Bw.Entities;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class WeaponDrawClientHandler //TODO: смена оружия ждёт ответа сервера (полный пинг): мгновенной она станет, когда выбор слота войдёт в предсказываемую симуляцию
    {
        private const KeyCode SwapKey = KeyCode.Q;

        private readonly IReadonlyWeaponDraw _draw;
        private readonly INetRequestSender<JetBrains.Core.Unit> _request;
        private readonly KeyCode _slotKey;

        public WeaponDrawClientHandler(
            Lifetime lifetime,
            IOwnership ownership,
            IReadonlyWeaponDraw draw,
            INetRequestSender<JetBrains.Core.Unit> request,
            KeyCode slotKey)
        {
            _draw = draw;
            _request = request;
            _slotKey = slotKey;

            ownership.Mine.WhenTrue(lifetime, mineLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, mineLifetime).Subscribe(_ => UpdateDraw()));
        }

        private void UpdateDraw()
        {
            if (_draw.DrawnLifetime.Value.IsAlive)
                return;

            if (Input.GetKeyDown(_slotKey) || Input.GetKeyDown(SwapKey) || Input.mouseScrollDelta.y != 0f) //TODO: new input system; Q и колесо просят достать каждое убранное оружие — это смена, пока слотов два, с третьим следующее оружие должен выбирать инвентарь
                _request.Send(JetBrains.Core.Unit.Instance);
        }
    }
}
