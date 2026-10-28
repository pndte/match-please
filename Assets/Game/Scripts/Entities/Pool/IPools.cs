namespace Bw.Entities.Pool
{
    public interface IPools
    {
        public IPool<T> For<T>() where T : class;
        public IPool<T> For<T, TKey>(TKey key) where T : class;
    }
}
