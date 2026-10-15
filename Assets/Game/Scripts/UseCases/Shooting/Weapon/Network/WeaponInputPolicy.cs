using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Weapon.Extensions;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class WeaponInputPolicy : IInputPolicy<WeaponInput>
    {
        private readonly LagCompensationConfig _lagCompensation;
        private readonly ISimulationStep _step;

        public WeaponInputPolicy(LagCompensationConfig lagCompensation, ISimulationStep step)
        {
            _lagCompensation = lagCompensation;
            _step = step;
        }

        public bool IsValid(WeaponInput input) =>
            float.IsFinite(input.Aim)
            && float.IsFinite(input.ViewDelay)
            && float.IsFinite(input.PredictedDelay)
            && float.IsFinite(input.Press.ViewDelay)
            && float.IsFinite(input.Press.PredictedDelay);

        public WeaponInput Substitute(WeaponInput lastInput) => //TODO: в тиках без ввода сервер угадывает курок по последнему известному вводу: зажатый остаётся зажатым (если игрок отпустил кнопку ровно в этих тиках, сервер выстрелит лишний раз, а сильный удар кулаков сорвётся позже, чем у клиента), а свежее нажатие — отпущенным (если игрок держал кнопку, очередь на сервере начнётся позже, а заряд сильного удара сорвётся)
            lastInput.WithoutPresses();

        public WeaponInput CaughtUp(
            WeaponInput input,
            WeaponInput lastInput,
            WeaponInput heard,
            int heardTicksAgo)
        {
            var press = heard.Press; //TODO: ввод несёт только последнее нажатие курка: если в одну дыру (пакеты опоздали или пропали) попали два клика, сервер узнает время только второго, и первый выстрел пропадёт
            var lateTicks = heardTicksAgo + press.Age;
            if (press.Number == lastInput.Press.Number || lateTicks < 0)
                return input.WithPress(lastInput.Press.WithAge(lastInput.Press.Age + 1));

            return lateTicks <= _lagCompensation.RewindTicks(_step) //TODO: нажатие старше предела отката (MaxRewindSeconds, 0,4 с) не спасается: сервер уже не может поставить цели туда, где их видел стрелок, и клик пропадает, как раньше
                ? input.Pressed(press, lateTicks)
                : input.WithPress(press.WithAge(lateTicks));
        }
    }
}
