namespace Bw.Entities.Network.Ticks
{
    public static class ScheduledCollectionExtensions
    {
        public static void PoseAt(this IScheduledCollection scheduled, double tick)
        {
            for (var index = 0; index < scheduled.Count; index++)
                scheduled[index].PoseAt(tick);
        }
    }
}
