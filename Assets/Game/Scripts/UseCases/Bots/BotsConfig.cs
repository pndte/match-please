using System;
using Bw.UseCases.Combat.Melee;
using UnityEngine;

namespace Bw.UseCases.Bots
{
    [CreateAssetMenu(fileName = "BotsConfig", menuName = "Configs/BotsConfig")]
    public sealed class BotsConfig : ScriptableObject
    {
        public BotEntry[] Bots = Array.Empty<BotEntry>();
        public LayerMask SightBlockers = 1 << 14;
        public Vector2 SightHalfExtents = new(13f, 7.3f);
        public MeleeWeaponConfig Fists;
        public BotDifficultyConfig StandIn;
        [Min(1f)] public float PropSearchRange = 12f;
        [Min(0f)] public float RestingPropSpeed = 1f;

        [Header("Plan costs")]
        [Min(0.1f)] public float ShootCost = 1f;
        [Min(0.1f)] public float DrawCost = 1f;
        [Min(0.1f)] public float ApproachCost = 2f;
        [Min(0.1f)] public float CloseInCost = 2f;
        [Min(0.1f)] public float PunchCost = 1f;
        [Min(0.1f)] public float ChargedPunchCost = 2.5f;
        [Min(0.1f)] public float FetchPropCost = 2f;
        [Min(0.1f)] public float ShieldCost = 1.5f;
        [Min(0.1f)] public float ThrowCost = 1f;
        [Min(0.1f)] public float RoamCost = 1f;
    }
}
