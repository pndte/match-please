using System.Globalization;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bw.UseCases.Match.View.Hud
{
    public sealed class ScoreboardView
    {
        private const string Hidden = "hidden";
        private const KeyCode HoldKey = KeyCode.Tab;

        private readonly IMatchViewModel _match;
        private readonly VisualTreeAsset _rowTemplate;
        private ScoreSheet _drawn = ScoreSheet.None;

        public ScoreboardView(
            Lifetime lifetime,
            HudDocument hud,
            VisualTreeAsset rowTemplate,
            IMatchViewModel match)
        {
            _match = match;
            _rowTemplate = rowTemplate;

            hud.WhenShown(lifetime, (shownLifetime, root) =>
            {
                var layer = root.Q("scoreboard-layer");
                var rows = layer.Q("rows");
                Observable.EveryUpdate(UnityFrameProvider.Update, shownLifetime).Subscribe(_ => Refresh(layer, rows));
            });
        }

        private void Refresh(VisualElement layer, VisualElement rows)
        {
            var sheet = _match.Sheet.Value;
            var shown = Input.GetKey(HoldKey) && sheet.Stage != MatchStage.Ended; //TODO: new input system
            layer.EnableInClassList(Hidden, !shown);
            if (!shown || ReferenceEquals(sheet, _drawn))
                return;

            _drawn = sheet;
            Draw(rows, sheet);
        }

        private void Draw(VisualElement rows, ScoreSheet sheet)
        {
            rows.Clear();
            for (var index = 0; index < sheet.Lines.Count; index++)
            {
                var line = sheet.Lines[index];
                var row = _rowTemplate.Instantiate().Q("row");
                row.EnableInClassList("score-row--leader", index == 0 && line.Stats.Kills > 0);
                row.EnableInClassList("score-row--me", line.Mine);
                row.EnableInClassList("score-row--odd", index % 2 == 1 && !line.Mine);
                row.Q<Label>("place").text = (index + 1).ToString(CultureInfo.InvariantCulture);
                row.Q<Label>("nickname").text = line.Nickname;
                ScoreLabels.Fill(row, line.Stats);
                rows.Add(row);
            }
        }
    }
}
