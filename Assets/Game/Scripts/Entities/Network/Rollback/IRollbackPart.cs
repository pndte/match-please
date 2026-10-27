namespace Bw.Entities.Network.Rollback
{
    public interface IRollbackPart : IDivergenceSource
    {
        public void Save(int tick);
        public void Follow(int tick);
        public void Restore(int tick);
    }
}
