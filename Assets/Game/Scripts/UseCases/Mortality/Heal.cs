namespace Bw.UseCases.Mortality
{
    public readonly struct Heal
    {
        public readonly float Amount;

        public Heal(float amount)
        {
            Amount = amount;
        }
    }
}
