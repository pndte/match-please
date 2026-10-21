using System.Collections.Generic;
using Bw.UseCases.Bots.Goap;
using Bw.UseCases.Bots.Senses;
using Bw.UseCases.Bots.Senses.Extensions;
using UnityEngine;

namespace Bw.UseCases.Bots.Abstractions
{
    public readonly struct Perception
    {
        public readonly Vector2 Eye;
        public readonly BotFact Facts;

        private readonly IReadOnlyList<Sighting> _sightings;

        public Perception(Vector2 eye, IReadOnlyList<Sighting> sightings, BotFact facts)
        {
            Eye = eye;
            _sightings = sightings;
            Facts = facts;
        }

        public bool TryFindTarget(out Sighting target) =>
            _sightings.TryFindTarget(Eye, out target);
    }
}
