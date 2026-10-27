using System;
using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Bots.Extensions;
using Bw.UseCases.Bots.Goap;
using Bw.UseCases.Bots.Navigation;
using UnityEngine;

namespace Bw.UseCases.Bots.Skills
{
    public sealed class RetreatSkill : IBotSkill //TODO: бот отходит к узлу подальше от цели, а путь к этому узлу может пройти мимо неё
    {
        public SkillStatus Status { get; private set; }

        private readonly BotNavigator _navigator;
        private readonly NavGraph _graph;
        private readonly BotGunner _gunner;
        private readonly IBotGamepad _gamepad;
        private readonly BotsConfig _bots;

        public RetreatSkill(
            BotNavigator navigator,
            NavGraph graph,
            BotGunner gunner,
            IBotGamepad gamepad,
            BotsConfig bots)
        {
            _navigator = navigator;
            _graph = graph;
            _gunner = gunner;
            _gamepad = gamepad;
            _bots = bots;
        }

        public void Begin()
        {
            Status = SkillStatus.Running;
            _navigator.Stop();
            _gunner.Begin();
        }

        public void Perform(Perception perception)
        {
            if (!perception.TryFindTarget(out var target))
                throw new InvalidOperationException("The retreat skill backs away from a noticed target: the brain runs it only while the bot knows of one.");

            if (_navigator.Status != NavStatus.Moving && !TryHeadAway(perception.Eye, target.Position))
            {
                _navigator.Stop();
                Status = SkillStatus.Failed;
                return;
            }

            _navigator.Steer();
            if ((perception.Facts & BotFact.GunDrawn) != 0)
                _gunner.FireAt(perception.Eye, target);
            else
                _gamepad.AimAt(perception.Eye, target.Position, 0f);
        }

        private bool TryHeadAway(Vector2 eye, Vector2 target)
        {
            var farthest = (eye - target).sqrMagnitude;
            var found = false;
            var destination = eye;
            for (var node = 0; node < _graph.NodeCount; node++)
            {
                var position = _graph.NodeAt(node);
                var away = (position - target).sqrMagnitude;
                if (away <= farthest || (position - eye).sqrMagnitude > _bots.RetreatRange * _bots.RetreatRange)
                    continue;

                farthest = away;
                destination = position;
                found = true;
            }

            return found && _navigator.TryGoTo(destination);
        }
    }
}
