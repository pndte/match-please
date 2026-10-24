namespace Bw.Entities.Network.Rollback
{
    public interface IRollbackPart
    {
        public bool TryGetDivergence(out int tick);
        public void Save(int tick);
        public void Restore(int tick);
    }
}
