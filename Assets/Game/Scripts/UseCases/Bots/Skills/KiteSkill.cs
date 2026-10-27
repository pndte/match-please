using System;
using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Bots.Navigation;
using Bw.UseCases.Bots.Senses;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Bw.UseCases.Bots.Skills
{
    public sealed class KiteSkill : IBotSkill //TODO: из точки для обхода цель видна, когда бот её выбирает; пока он идёт туда, цель может уйти за стену
    {
        public SkillStatus Status { get; private set; }

        private readonly BotNavigator _navigator;
        private readonly NavGraph _graph;
        private readonly BotSight _sight;
        private readonly BotGunner _gunner;
        private readonly BotsConfig _bots;

        private float _side;

        public KiteSkill(
            BotNavigator navigator,
            NavGraph graph,
            BotSight sight,
            BotGunner gunner,
            BotsConfig bots)
        {
            _navigator = navigator;
            _graph = graph;
            _sight = sight;
            _gunner = gunner;
            _bots = bots;
        }

        public void Begin()
        {
            Status = SkillStatus.Running;
            _navigator.Stop();
            _gunner.Begin();
            _side = Random.value < 0.5f ? -1f : 1f;
        }

        public void Perform(Perception perception)
        {
            if (!perception.TryFindTarget(out var target))
                throw new InvalidOperationException("The kite skill keeps its distance from a target in sight: the brain runs it only while the bot sees one.");

            if (_navigator.Status != NavStatus.Moving && !TryHeadAside(perception.Eye, target.Position))
            {
                _navigator.Stop();
                Status = SkillStatus.Failed;
                return;
            }

            _navigator.Steer();
            _gunner.FireAt(perception.Eye, target);
        }

        private bool TryHeadAside(Vector2 eye, Vector2 target)
        {
            _side = -_side;
            if (TryFindSpot(eye, target, out var spot))
                return _navigator.TryGoTo(spot);

            _side = -_side;
            return TryFindSpot(eye, target, out spot) && _navigator.TryGoTo(spot);
        }

        private bool TryFindSpot(Vector2 eye, Vector2 target, out Vector2 spot)
        {
            var flank = Mathf.Sign(eye.x - target.x);
            var nearest = float.MaxValue;
            spot = eye;
            for (var node = 0; node < _graph.NodeCount; node++)
            {
                var position = _graph.NodeAt(node);
                var range = (position - target).magnitude;
                if (range < _bots.KiteNear || range > _bots.KiteFar || Mathf.Sign(position.x - target.x) != flank || (position.x - eye.x) * _side < _bots.KiteStride)
                    continue;

                var walk = (position - eye).sqrMagnitude;
                if (walk >= nearest || !_sight.IsInView(position, target))
                    continue;

                nearest = walk;
                spot = position;
            }

            return nearest < float.MaxValue;
        }
    }
}
