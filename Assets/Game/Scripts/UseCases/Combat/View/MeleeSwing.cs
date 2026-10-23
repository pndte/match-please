namespace Bw.UseCases.Combat.View
{
    public readonly struct MeleeSwing
    {
        public readonly int Number;
        public readonly bool Strong;

        public MeleeSwing(int number, bool strong)
        {
            Number = number;
            Strong = strong;
        }
    }
}
