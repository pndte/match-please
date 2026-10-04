using System.Collections.Generic;

namespace Bw.Entities.Network.Variables
{
    public sealed class NetEntriesSchema
    {
        internal IReadOnlyList<NetEntryDeclaration> Declarations { get; }

        internal NetEntriesSchema(IReadOnlyList<NetEntryDeclaration> declarations)
        {
            Declarations = declarations;
        }
    }
}
