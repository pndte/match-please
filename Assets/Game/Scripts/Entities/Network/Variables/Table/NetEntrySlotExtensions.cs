using System.Collections.Generic;

namespace Bw.Entities.Network.Variables
{
    internal static class NetEntrySlotExtensions
    {
        public static NetworkMessageHeader HeaderOn(this NetEntrySlot slot, ulong networkObjectId) =>
            new()
            {
                NetworkObjectId = networkObjectId,
                VarId = slot.VarId
            };

        public static void ForgetPending(this IReadOnlyList<NetEntrySlot> slots)
        {
            for (var index = 0; index < slots.Count; index++)
                slots[index].Entry.Dirty.Value = false;
        }
    }
}
