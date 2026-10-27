using Bw.UseCases.Combat.Abstractions.Characters;

namespace Bw.UseCases.Bots.Senses
{
    public readonly struct Threat
    {
        public readonly IReadonlyCharacter Attacker;
        public readonly int Heard;
        public readonly int SinceTicks;
        public readonly int QuietTicks;
        public readonly bool Noticed;

        public Threat(
            IReadonlyCharacter attacker,
            int heard,
            int sinceTicks,
            int quietTicks,
            bool noticed)
        {
            Attacker = attacker;
            Heard = heard;
            SinceTicks = sinceTicks;
            QuietTicks = quietTicks;
            Noticed = noticed;
        }
    }
}
