namespace Bw.Entities.Pool
{
    public interface IPools
    {
        public IPool<T> For<T>();
    }
}
