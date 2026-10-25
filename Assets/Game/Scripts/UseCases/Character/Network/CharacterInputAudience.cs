using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Bubble;
using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Combat.Abstractions.Characters;

namespace Bw.UseCases.Character.Network
{
    public sealed class CharacterInputAudience : IInputAudience
    {
        private readonly IReadonlyCharacter _character;
        private readonly ICharacterRegistry _registry;
        private readonly IReadonlyPlayerDrivers _drivers;
        private readonly PredictionBubbleConfig _bubble;

        public CharacterInputAudience(
            IReadonlyCharacter character,
            ICharacterRegistry registry,
            IReadonlyPlayerDrivers drivers,
            PredictionBubbleConfig bubble)
        {
            _character = character;
            _registry = registry;
            _drivers = drivers;
            _bubble = bubble;
        }

        public void Collect(List<IClient> clients)
        {
            var position = _character.Movement.State.Value.Body.Position;
            foreach (var (watcher, player) in _registry.PlayerByCharacter)
                if (watcher != _character && _bubble.NeedsInputs(watcher.Movement.State.Value.Body.Position, position))
                    _drivers.Of(player).Value.Switch(clients, (watching, client) => AddOnce(watching, client), _ => { });
        }

        private static void AddOnce(List<IClient> clients, IClient client)
        {
            if (!clients.Contains(client))
                clients.Add(client);
        }
    }
}
