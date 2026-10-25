using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Bubble;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.Network
{
    public sealed class ClientBubbles : IClientBubbles
    {
        private readonly List<(IClient Client, Vector2 Center)> _centers = new();
        private readonly ICharacterRegistry _registry;
        private readonly IReadonlyPlayerDrivers _drivers;
        private readonly PredictionBubbleConfig _config;

        public ClientBubbles(
            Lifetime lifetime,
            IWorldStep worldStep,
            ICharacterRegistry registry,
            IReadonlyPlayerDrivers drivers,
            PredictionBubbleConfig config)
        {
            _registry = registry;
            _drivers = drivers;
            _config = config;

            worldStep.Stepped(TickPhase.Late).Advise(lifetime, _ => Gather());
        }

        public bool Contains(IClient client, Vector2 position, bool inside)
        {
            var radius = inside ? _config.LeaveRadius : _config.EnterRadius;
            foreach (var (owner, center) in _centers)
                if (owner == client && (center - position).sqrMagnitude <= radius * radius)
                    return true;

            return false;
        }

        private void Gather()
        {
            _centers.Clear();
            foreach (var (character, player) in _registry.PlayerByCharacter)
                _drivers.Of(player).Value.Switch((Centers: _centers, Center: character.Movement.State.Value.Body.Position),
                    (state, client) => state.Centers.Add((client, state.Center)), _ => { });
        }
    }
}
