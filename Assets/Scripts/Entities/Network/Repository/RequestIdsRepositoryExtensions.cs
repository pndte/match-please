namespace Bw.Entities.Network.Repository
{
    public static class RequestIdsRepositoryExtensions
    {
        public static bool TryRemoveIdFor<T>(this IRequestIdsRepository requestIdsRepository, ulong requestId)
        {
            if (!requestIdsRepository.ContainsIdFor<T>(requestId)) return false;
            
            requestIdsRepository.RemoveIdFor<T>(requestId);
            return true;
        }
    }
}