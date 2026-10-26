using Bw.Entities.Network.Variables;
using Bw.UseCases.Match.Network.Requests;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Match.Network
{
    public sealed class MatchTableWriter
    {
        private readonly MatchTables _tables;
        private readonly IViewableProperty<MatchTable> _table;

        public MatchTableWriter(MatchTables tables, INetEntries entries, NetPropertyDeclaration<MatchTable> declaration)
        {
            _tables = tables;
            _table = entries.Property(declaration);
        }

        public void Write() =>
            _table.Value = _tables.Create();
    }
}
