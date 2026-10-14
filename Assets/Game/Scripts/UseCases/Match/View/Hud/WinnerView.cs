using System;
using System.Globalization;
using System.Linq;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine.UIElements;

namespace Bw.UseCases.Match.View.Hud
{
    public sealed class WinnerView
    {
        private const string Hidden = "hidden";
        private const string FigurePrefix = "winner__figure--";

        private readonly IReadonlyMatchBoard _board;
        private readonly MatchCountdown _countdown;
        private int _shownSeconds = -1;

        public WinnerView(
            Lifetime lifetime,
            HudDocument hud,
            IReadonlyMatchBoard board,
            MatchCountdown countdown)
        {
            _board = board;
            _countdown = countdown;

            hud.WhenShown(lifetime, (shownLifetime, root) =>
            {
                var feed = root.Q("kill-feed-layer");
                var layer = root.Q("winner-layer");
                var next = layer.Q<Label>("next");
                board.Current.Advise(shownLifetime, standings => Show(feed, layer, standings));
                Observable.EveryUpdate(UnityFrameProvider.Update, shownLifetime).Subscribe(_ => Count(next));
            });
        }

        private void Show(VisualElement feed, VisualElement layer, MatchStandings standings)
        {
            var ended = standings.Period.Phase == MatchPhase.Ended && standings.Lines.Count > 0;
            feed.EnableInClassList(Hidden, ended);
            layer.EnableInClassList(Hidden, !ended);
            if (!ended)
                return;

            var winner = standings.Lines[0];
            layer.Q<Label>("title").text = winner.IsMine ? "Ты победил!" : "Победитель матча";
            var figure = layer.Q("figure");
            foreach (var shown in figure.GetClasses().Where(name => name.StartsWith(FigurePrefix, StringComparison.Ordinal)).ToArray())
                figure.RemoveFromClassList(shown);

            figure.AddToClassList(FigurePrefix + winner.Figure);
            layer.Q<Label>("nickname").text = winner.Nickname;
            ScoreLabels.Fill(layer, winner.Score);
            _shownSeconds = -1;
        }

        private void Count(Label next)
        {
            if (_board.Current.Value.Period.Phase != MatchPhase.Ended)
                return;

            var seconds = _countdown.SecondsLeft;
            if (seconds == _shownSeconds)
                return;

            _shownSeconds = seconds;
            next.text = $"Новый матч через {seconds.ToString(CultureInfo.InvariantCulture)} с";
        }
    }
}
