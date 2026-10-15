using System;
using UnityEngine;

namespace Bw.CoreAdapters.Characters
{
    [CreateAssetMenu(fileName = "CharacterLooksConfig", menuName = "Configs/CharacterLooksConfig")]
    public sealed class CharacterLooksConfig : ScriptableObject
    {
        public CharacterLook[] Looks = Array.Empty<CharacterLook>();
    }
}
