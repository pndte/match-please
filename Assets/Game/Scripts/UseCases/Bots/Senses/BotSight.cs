using System.Collections.Generic;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Bots.Senses.Extensions;
using Bw.UseCases.Character;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Mortality;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Bots.Senses
{
    public sealed class BotSight //TODO: слух пока не робит,
                                 // выстрел за стеной или удар в спину замечается, только когда стрелок окажется на виду
    
    {
        public Vector2 Eye => _characters[_self].transform.position;

        private readonly IReadonlyCharacter _self;
        private readonly IGameObjectByCharacterCollection _characters;
        private readonly PhysicsWorld _world;
        private readonly PhysicsQuery.QueryFilter _blockers;
        private readonly Vector2 _frame;
        private readonly BotDifficultyConfig _config;
        private readonly ISimulationStep _step;

        public BotSight(
            IReadonlyCharacter self,
            IGameObjectByCharacterCollection characters,
            IPhysicsSpace space,
            BotsConfig bots,
            BotDifficultyConfig config,
            ISimulationStep step)
        {
            _self = self;
            _characters = characters;
            _world = space.World;
            _blockers = new PhysicsQuery.QueryFilter(PhysicsMask.All, bots.SightBlockers.AsPhysicsMask());
            _frame = bots.SightHalfExtents;
            _config = config;
            _step = step;
        }

        public IReadOnlyList<Sighting> SightingsAfter(IReadOnlyList<Sighting> remembered)
        {
            var eye = Eye;
            var reaction = Ticks(_config.ReactionTime);
            var sightings = StillRemembered(remembered);
            foreach (var pair in _characters)
            {
                var character = pair.Key;
                if (character == _self || character.Mortal.State.Value != LifeState.Alive)
                    continue;

                var position = (Vector2)pair.Value.transform.position;
                var visible = IsInFrame(position - eye) && IsClear(eye, position);
                var index = IndexOf(sightings, character);
                if (index >= 0)
                    sightings[index] = visible ? sightings[index].Seen(position, reaction) : sightings[index].Unseen();
                else if (visible)
                    sightings.Add(new Sighting(character, position, true, reaction <= 1, 1, 0));
            }

            return sightings;
        }

        private List<Sighting> StillRemembered(IReadOnlyList<Sighting> remembered)
        {
            var memory = Ticks(_config.MemoryTime);
            var kept = new List<Sighting>(remembered.Count + 1);
            for (var index = 0; index < remembered.Count; index++)
            {
                var sighting = remembered[index];
                if (_characters.ContainsKey(sighting.Character) && sighting.Character.Mortal.State.Value == LifeState.Alive && sighting.UnseenTicks <= memory)
                    kept.Add(sighting);
            }

            return kept;
        }

        private bool IsInFrame(Vector2 offset) =>
            Mathf.Abs(offset.x) <= _frame.x && Mathf.Abs(offset.y) <= _frame.y;

        private bool IsClear(Vector2 from, Vector2 to)
        {
            using var hits = _world.CastRay(new PhysicsQuery.CastRayInput(from, to - from), _blockers, PhysicsQuery.WorldCastMode.AllSorted, Allocator.Temp);
            foreach (var hit in hits)
                if (!hit.shape.IsOneWay())
                    return false;

            return true;
        }

        private int Ticks(float seconds) =>
            Mathf.CeilToInt(seconds / _step.Duration);

        private static int IndexOf(List<Sighting> sightings, IReadonlyCharacter character)
        {
            for (var index = 0; index < sightings.Count; index++)
                if (sightings[index].Character == character)
                    return index;

            return -1;
        }
    }
}
