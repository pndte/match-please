namespace Bw.UseCases.Mortality.Guarding
{
    public readonly struct GuardStance
    {
        public static readonly GuardStance Lowered = new(false, 0f);

        public readonly bool Raised;
        public readonly float Aim;

        public GuardStance(bool raised, float aim)
        {
            Raised = raised;
            Aim = aim;
        }
    }
}
