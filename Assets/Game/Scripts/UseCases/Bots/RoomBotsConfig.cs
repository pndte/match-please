using System;
using UnityEngine;

namespace Bw.UseCases.Bots
{
    [CreateAssetMenu(fileName = "RoomBotsConfig", menuName = "Configs/RoomBotsConfig")]
    public sealed class RoomBotsConfig : ScriptableObject
    {
        [Min(0)] public int MaxBots = 8;
        public string[] Names = Array.Empty<string>();

        [Header("Bots of a server started without the room menu (Multiplayer Play Mode tags, -server without -bots)")]
        public StartingBot[] StartingBots = Array.Empty<StartingBot>();
    }
}
