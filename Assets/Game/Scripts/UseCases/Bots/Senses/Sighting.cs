using Bw.UseCases.Combat.Abstractions.Characters;
using UnityEngine;

namespace Bw.UseCases.Bots.Senses
{
    public readonly struct Sighting
    {
        public readonly IReadonlyCharacter Character;
        public readonly Vector2 Position;
        public readonly bool Visible;
        public readonly bool Noticed;
        public readonly int SeenTicks;
        public readonly int UnseenTicks;

        public Sighting(
            IReadonlyCharacter character,
            Vector2 position,
            bool visible,
            bool noticed,
            int seenTicks,
            int unseenTicks)
        {
            Character = character;
            Position = position;
            Visible = visible;
            Noticed = noticed;
            SeenTicks = seenTicks;
            UnseenTicks = unseenTicks;
        }
    }
}
