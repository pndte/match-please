namespace Bw.Entities.Pool
{
    public sealed class PoolSettings
    {
        public readonly int Prewarm;
        public readonly int Limit;
        public readonly PoolCap Cap;

        public PoolSettings(int prewarm, int limit, PoolCap cap)
        {
            Prewarm = prewarm;
            Limit = limit;
            Cap = cap;
        }
    }
}
