using System;
using Bw.UseCases.Character;
using UnityEngine;

namespace Bw.CoreAdapters.Characters
{
    [Serializable]
    public struct CharacterLook
    {
        public CharacterDescription Character;
        public string DisplayName;
        public Sprite Portrait;
    }
}
