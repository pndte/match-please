using JetBrains.Lifetimes;

namespace Bw.Entities.Pool
{
    public static class PoolExtensions
    {
        public static T OneShot<T>(this IPool<T> pool, Lifetime lifetime) where T : IOneShot
        {
            var shot = lifetime.CreateNested();
            try
            {
                var facade = pool.Resource(shot.Lifetime);
                facade.Finished.Advise(shot.Lifetime, _ => shot.Terminate());
                return facade;
            }
            catch
            {
                shot.Terminate();
                throw;
            }
        }
    }
}
