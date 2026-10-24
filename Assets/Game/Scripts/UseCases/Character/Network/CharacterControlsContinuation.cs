using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon.Network.Extensions;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Movement.Network.Requests;

namespace Bw.UseCases.Character.Network
{
    public sealed class CharacterControlsContinuation : IInputContinuation<CharacterControls>
    {
        public CharacterControls Continued(CharacterControls last, int ticks) //TODO: ввод, которого нет (сервер не услышал тик, клиент ещё не получил чужой), угадывается по последнему известному: зажатый курок остаётся зажатым (если игрок отпустил кнопку ровно в этих тиках, сервер выстрелит лишний раз, а сильный удар кулаков сорвётся позже, чем у клиента), а свежее нажатие — отпущенным (если игрок держал кнопку, очередь на сервере начнётся позже, а заряд сильного удара сорвётся); выбор оружия живёт один тик: если пропал и его тик, и следующий пакет с копией, сервер его не узнает, и клиент вернётся к прежнему оружию при сверке; нужен счётчик выборов, как у нажатий курка
        {
            var movement = last.Movement;
            var hands = last.Hands;
            return new CharacterControls(
                new MovementControls(movement.Horizontal, false, movement.JumpHeld, movement.Down),
                new HandsControls(hands.Aim, Aged(hands.Trigger.WithoutFreshPress(), ticks), false, DrawControl.Keep),
                last.Guard);
        }

        private static TriggerControls Aged(TriggerControls trigger, int ticks) =>
            trigger.WithPress(trigger.Press.WithAge(trigger.Press.Age + ticks));
    }
}
