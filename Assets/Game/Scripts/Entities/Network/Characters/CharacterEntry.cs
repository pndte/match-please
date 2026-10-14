using System;
using Unity.Netcode;

namespace Bw.Entities.Network.Characters
{
    [Serializable]
    public struct CharacterEntry
    {
        public string Key;
        public NetworkObject Prefab;
    }
}
