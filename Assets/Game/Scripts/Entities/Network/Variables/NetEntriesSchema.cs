using System.Collections.Generic;

namespace Bw.Entities.Network.Variables
{
    public sealed class NetEntriesSchema
    {
        internal IReadOnlyList<NetEntryDeclaration> Declarations { get; }
        internal uint Hash { get; }

        private const uint FnvOffsetBasis = 2166136261;
        private const uint FnvPrime = 16777619;

        internal NetEntriesSchema(IReadOnlyList<NetEntryDeclaration> declarations)
        {
            Declarations = declarations;
            Hash = HashOf(declarations);
        }

        private static uint HashOf(IReadOnlyList<NetEntryDeclaration> declarations) //TODO: одинаковые объявления (тип, доставка, права) хеш не различает — перестановку сигналов владения и управления он не поймает; различать по месту объявления или по имени
        {
            var hash = FnvOffsetBasis;
            for (var index = 0; index < declarations.Count; index++)
            {
                var declaration = declarations[index];
                hash = Mix(hash, declaration.GetType().ToString());
                hash = Mix(hash, (uint)declaration.DeliveryType);
                hash = Mix(hash, (uint)declaration.Permissions);
            }

            return hash;
        }

        private static uint Mix(uint hash, string text)
        {
            for (var index = 0; index < text.Length; index++)
                hash = Mix(hash, text[index]);

            return hash;
        }

        private static uint Mix(uint hash, uint value) =>
            (hash ^ value) * FnvPrime;
    }
}
