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
                facade.Finished.Advise(shot.Lifetime, _ => shot.Terminate()); //TODO: если пул отберёт выдачу по потолку (ReclaimOldest), Finished не придёт и shot доживёт до конца lifetime — небольшая утечка на каждую отобранную выдачу
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
