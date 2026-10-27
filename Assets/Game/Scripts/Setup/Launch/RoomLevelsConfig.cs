using System;
using UnityEngine;

namespace Setup.Launch
{
    [CreateAssetMenu(fileName = "RoomLevelsConfig", menuName = "Configs/RoomLevelsConfig")]
    public sealed class RoomLevelsConfig : ScriptableObject
    {
        public RoomLevel[] Levels = Array.Empty<RoomLevel>();
    }

    [Serializable]
    public struct RoomLevel
    {
        public string Scene;
        public string DisplayName;
    }
}
