namespace Bw.Entities.Pool
{
    internal interface IKeyedPools<in TKey>
    {
        public IPool<T> For<T>(TKey key) where T : class;
    }
}
