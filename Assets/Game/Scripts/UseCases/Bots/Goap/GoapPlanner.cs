using System.Collections.Generic;
using Bw.UseCases.Bots.Goap.Extensions;

namespace Bw.UseCases.Bots.Goap
{
    public sealed class GoapPlanner
    {
        private const int MaxDepth = 6;

        private readonly IReadOnlyList<GoapAction> _actions;
        private readonly List<Step> _steps = new();
        private readonly List<int> _open = new();
        private readonly Dictionary<BotFact, float> _cheapest = new();

        public GoapPlanner(IReadOnlyList<GoapAction> actions)
        {
            _actions = actions;
        }

        public bool TryPlan(BotFact facts, BotFact wanted, List<GoapAction> plan)
        {
            plan.Clear();
            _steps.Clear();
            _open.Clear();
            _cheapest.Clear();
            _steps.Add(new Step(facts, 0f, 0, -1, null));
            _open.Add(0);
            _cheapest[facts] = 0f;
            var cheapestAction = CheapestCost();
            while (_open.Count > 0)
            {
                var current = PopBest(wanted, cheapestAction);
                var step = _steps[current];
                if ((step.Facts & wanted) == wanted)
                {
                    Unwind(current, plan);
                    return true;
                }

                if (step.Depth >= MaxDepth)
                    continue;

                foreach (var action in _actions)
                {
                    if (!action.When.IsMetBy(step.Facts))
                        continue;

                    var next = action.Then.AppliedTo(step.Facts);
                    var cost = step.Cost + action.Cost;
                    if (next == step.Facts || _cheapest.TryGetValue(next, out var known) && known <= cost)
                        continue;

                    _cheapest[next] = cost;
                    _steps.Add(new Step(next, cost, step.Depth + 1, current, action));
                    _open.Add(_steps.Count - 1);
                }
            }

            return false;
        }

        private int PopBest(BotFact wanted, float cheapestAction)
        {
            var best = 0;
            for (var index = 1; index < _open.Count; index++)
                if (Estimate(_steps[_open[index]], wanted, cheapestAction) < Estimate(_steps[_open[best]], wanted, cheapestAction))
                    best = index;

            var popped = _open[best];
            _open.RemoveAt(best);
            return popped;
        }

        private float CheapestCost()
        {
            var cheapest = float.MaxValue;
            foreach (var action in _actions)
                if (action.Cost < cheapest)
                    cheapest = action.Cost;

            return cheapest == float.MaxValue ? 0f : cheapest;
        }

        private void Unwind(int last, List<GoapAction> plan)
        {
            for (var index = last; _steps[index].Parent >= 0; index = _steps[index].Parent)
                plan.Add(_steps[index].Action);

            plan.Reverse();
        }

        private static float Estimate(Step step, BotFact wanted, float cheapestAction) =>
            step.Cost + Missing(step.Facts, wanted) * cheapestAction;

        private static int Missing(BotFact facts, BotFact wanted)
        {
            var missing = (int)(wanted & ~facts);
            var count = 0;
            while (missing != 0)
            {
                missing &= missing - 1;
                count++;
            }

            return count;
        }

        private readonly struct Step
        {
            public readonly BotFact Facts;
            public readonly float Cost;
            public readonly int Depth;
            public readonly int Parent;
            public readonly GoapAction Action;

            public Step(
                BotFact facts,
                float cost,
                int depth,
                int parent,
                GoapAction action)
            {
                Facts = facts;
                Cost = cost;
                Depth = depth;
                Parent = parent;
                Action = action;
            }
        }
    }
}
