namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class InputGaps
    {
        public int Late { get; private set; }
        public int LateTicks { get; private set; }
        public int Early { get; private set; }
        public int Recovered { get; private set; }
        public int Substituted { get; private set; }

        internal void CountLate(int ticks)
        {
            Late++;
            LateTicks += ticks;
        }

        internal void CountEarly() =>
            Early++;

        internal void CountRecovered() =>
            Recovered++;

        internal void CountSubstituted() =>
            Substituted++;
    }
}
