using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Bots.Network.Requests;

namespace Bw.Injection.Match
{
    public readonly struct BotRosterEntries
    {
        public readonly NetPropertyDeclaration<BotTable> Table;
        public readonly NetRequestDeclaration<RoomBot> Joins;
        public readonly NetRequestDeclaration<int> Removals;
        public readonly NetRequestDeclaration<BotPresetChange> Changes;

        public BotRosterEntries(
            NetPropertyDeclaration<BotTable> table,
            NetRequestDeclaration<RoomBot> joins,
            NetRequestDeclaration<int> removals,
            NetRequestDeclaration<BotPresetChange> changes)
        {
            Table = table;
            Joins = joins;
            Removals = removals;
            Changes = changes;
        }
    }
}
