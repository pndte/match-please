using System;
using UnityEngine;

namespace Bw.CoreAdapters.Characters
{
    [CreateAssetMenu(fileName = "CharacterCatalog", menuName = "Configs/CharacterCatalog")]
    public sealed class CharacterCatalog : ScriptableObject
    {
        public CharacterEntry[] Characters = Array.Empty<CharacterEntry>();
    }
}
