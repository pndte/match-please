namespace Bw.Entities.Network.Rollback
{
    public interface IDivergenceSource
    {
        public bool TryGetDivergence(out int tick);
    }
}
