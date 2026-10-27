using System;
using System.Collections.Generic;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine.UIElements;

namespace Bw.UseCases.Bots.View.Roster
{
    public sealed class BotRosterView
    {
        private const string Hidden = "hidden";
        private const string Locked = "button--locked";

        private readonly VisualTreeAsset _row;
        private readonly VisualElement _rows;
        private readonly Label _count;
        private readonly IBotRosterViewModel _roster;

        public BotRosterView(Lifetime lifetime, VisualElement root, IBotRosterViewModel roster)
        {
            var sample = root.Q<TemplateContainer>("bot-sample");
            var add = root.Q<Button>("add-bot");
            _row = sample.templateSource;
            _rows = root.Q("bot-rows");
            _count = root.Q<Label>("bots-count");
            _roster = roster;

            sample.AddToClassList(Hidden);
            Action onAdd = Add;
            add.clicked += onAdd;
            lifetime.OnTermination(() => add.clicked -= onAdd);
            roster.Rows.Advise(lifetime, Show);
            roster.Full.Advise(lifetime, full => add.EnableInClassList(Locked, full));
        }

        private void Add()
        {
            if (!_roster.Full.Value)
                _roster.Add();
        }

        private void Show(IReadOnlyList<BotRow> rows)
        {
            _rows.Clear();
            for (var index = 0; index < rows.Count; index++)
                _rows.Add(RowOf(rows[index], index));

            _count.text = rows.Count.ToString();
        }

        private VisualElement RowOf(BotRow row, int index)
        {
            var element = _row.CloneTree();
            element.Q<Label>("bot-name").text = row.Nickname;
            var difficulty = element.Q<Button>("bot-difficulty");
            difficulty.text = row.Difficulty;
            difficulty.clicked += () => _roster.CycleDifficulty(index);
            var personality = element.Q<Button>("bot-personality");
            personality.text = row.Personality;
            personality.clicked += () => _roster.CyclePersonality(index);
            element.Q<Button>("bot-remove").clicked += () => _roster.Remove(index);
            return element;
        }
    }
}
