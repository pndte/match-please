namespace Bw.Entities.Network.Prediction.Stream
{
    public readonly struct InputGap
    {
        public static readonly InputGap Early = new(InputGapKind.Early, 0);
        public static readonly InputGap Recovered = new(InputGapKind.Recovered, 0);
        public static readonly InputGap Substituted = new(InputGapKind.Substituted, 0);

        public readonly InputGapKind Kind;
        public readonly int LateTicks;

        private InputGap(InputGapKind kind, int lateTicks)
        {
            Kind = kind;
            LateTicks = lateTicks;
        }

        public static InputGap Late(int ticks) =>
            new(InputGapKind.Late, ticks);
    }
}
