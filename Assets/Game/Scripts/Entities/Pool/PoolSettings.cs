namespace Bw.Entities.Pool
{
    internal sealed class PoolSettings
    {
        public readonly int Prewarm;
        public readonly int MaxIdle;
        public readonly PoolCap Cap;

        public PoolSettings(int prewarm, int maxIdle, PoolCap cap)
        {
            Prewarm = prewarm;
            MaxIdle = maxIdle;
            Cap = cap;
        }
    }
}
