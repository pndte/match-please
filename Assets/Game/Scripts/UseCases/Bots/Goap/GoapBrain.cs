using System;
using System.Collections.Generic;
using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Bots.Goap.Extensions;
using Bw.UseCases.Bots.Senses;

namespace Bw.UseCases.Bots.Goap
{
    public sealed class GoapBrain : IBotBrain
    {
        private const int RetryTicks = 15;

        private readonly List<GoapAction> _plan = new();
        private readonly int[] _cooldowns;

        private readonly BotSight _sight;
        private readonly BotSituation _situation;
        private readonly IReadOnlyList<IGoapGoal> _goals;
        private readonly GoapPlanner _planner;
        private readonly IBotGamepad _gamepad;

        private IReadOnlyList<Sighting> _sightings = Array.Empty<Sighting>();
        private BotFact _facts;
        private int _goal;
        private int _step;
        private bool _begun;

        public GoapBrain(
            BotSight sight,
            BotSituation situation,
            IReadOnlyList<IGoapGoal> goals,
            GoapPlanner planner,
            IBotGamepad gamepad)
        {
            _cooldowns = new int[goals.Count];
            _sight = sight;
            _situation = situation;
            _goals = goals;
            _planner = planner;
            _gamepad = gamepad;
        }

        public void Think()
        {
            _gamepad.Release();
            var eye = _sight.Eye;
            _sightings = _sight.SightingsAfter(_sightings);
            _facts = _situation.FactsAfter(_facts, eye, _sightings);
            CoolDown();
            if (!IsFollowing(_facts))
                Plan(_facts);

            if (_step < _plan.Count)
                Perform(_plan[_step], new Perception(eye, _sightings, _facts));
        }

        private bool IsFollowing(BotFact facts) =>
            _step < _plan.Count
            && _plan[_step].When.IsMetBy(facts)
            && !IsOutranked(facts);

        private bool IsOutranked(BotFact facts)
        {
            var priority = _goals[_goal].PriorityIn(facts);
            if (priority <= 0f)
                return true;

            for (var index = 0; index < _goals.Count; index++)
                if (index != _goal && _cooldowns[index] == 0 && !_goals[index].IsMetBy(facts) && _goals[index].PriorityIn(facts) > priority)
                    return true;

            return false;
        }

        private void Plan(BotFact facts)
        {
            _plan.Clear();
            _step = 0;
            _begun = false;
            var tried = 0; 
            for (var attempt = 0; attempt < _goals.Count; attempt++)
            {
                if (!TryPickGoal(facts, tried, out var goal))
                    return;

                tried |= 1 << goal;
                if (_planner.TryPlan(facts, _goals[goal].Wanted, _plan) && _plan.Count > 0)
                {
                    _goal = goal;
                    return;
                }
            }
        }

        private bool TryPickGoal(BotFact facts, int tried, out int picked)
        {
            picked = -1;
            var best = 0f;
            for (var index = 0; index < _goals.Count; index++)
            {
                if ((tried & 1 << index) != 0 || _cooldowns[index] > 0 || _goals[index].IsMetBy(facts))
                    continue;

                var priority = _goals[index].PriorityIn(facts);
                if (priority <= best)
                    continue;

                best = priority;
                picked = index;
            }

            return picked >= 0;
        }

        private void Perform(GoapAction action, Perception perception)
        {
            if (!_begun)
            {
                action.Skill.Begin();
                _begun = true;
            }

            if (action.Skill.Status == SkillStatus.Running)
                action.Skill.Perform(perception);

            switch (action.Skill.Status)
            {
                case SkillStatus.Succeeded:
                    _step++;
                    _begun = false;
                    break;
                case SkillStatus.Failed:
                    _cooldowns[_goal] = RetryTicks;
                    _plan.Clear();
                    break;
            }
        }

        private void CoolDown()
        {
            for (var index = 0; index < _cooldowns.Length; index++)
                if (_cooldowns[index] > 0)
                    _cooldowns[index]--;
        }
    }
}
