using JetBrains.Lifetimes;
using R3;
using UnityEngine.UIElements;

namespace Bw.UseCases.Match.View.Hud
{
    public sealed class MatchTimerView
    {
        private const string Hidden = "hidden";

        private readonly IReadonlyMatchBoard _board;
        private readonly MatchCountdown _countdown;
        private int _shownSeconds = -1;

        public MatchTimerView(
            Lifetime lifetime,
            HudDocument hud,
            IReadonlyMatchBoard board,
            MatchCountdown countdown)
        {
            _board = board;
            _countdown = countdown;

            hud.WhenShown(lifetime, (shownLifetime, root) =>
            {
                var layer = root.Q("timer-layer");
                var time = layer.Q<Label>("match-timer");
                Observable.EveryUpdate(UnityFrameProvider.Update, shownLifetime).Subscribe(_ => Count(layer, time));
            });
        }

        private void Count(VisualElement layer, Label time)
        {
            var running = _board.Current.Value.Period.Phase == MatchPhase.Running;
            layer.EnableInClassList(Hidden, !running);
            if (!running)
                return;

            var seconds = _countdown.SecondsLeft;
            if (seconds == _shownSeconds)
                return;

            _shownSeconds = seconds;
            time.text = $"{seconds / 60}:{seconds % 60:00}";
        }
    }
}
