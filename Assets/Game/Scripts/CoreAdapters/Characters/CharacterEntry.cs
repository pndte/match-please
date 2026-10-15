using System;
using Bw.UseCases.Character;
using Unity.Netcode;

namespace Bw.CoreAdapters.Characters
{
    [Serializable]
    public struct CharacterEntry
    {
        public CharacterDescription Character;
        public NetworkObject Prefab;
    }
}
