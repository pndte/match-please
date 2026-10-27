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
        public BotPersonality StandInPersonality;
        [Min(1f)] public float PropSearchRange = 12f;
        [Min(0f)] public float RestingPropSpeed = 1f;

        [Header("Threats (a shot whose line passes this close to a bot, within the range, is an attack on it, and so is a blow charged within the charge range; it lasts the memory after the last one)")]
        [Min(0.1f)] public float ThreatRadius = 1.5f;
        [Min(1f)] public float ThreatRange = 25f;
        [Min(0.5f)] public float ChargeThreatRange = 3.5f;
        [Min(0f)] public float ThreatMemory = 0.4f;

        [Header("Retreat and kite")]
        [Min(1f)] public float RetreatRange = 15f;
        [Min(1f)] public float KiteNear = 7f;
        [Min(1f)] public float KiteFar = 12f;
        [Min(0.5f)] public float KiteStride = 2f;

        [Header("Plan costs")]
        [Min(0.1f)] public float ShootCost = 1f;
        [Min(0f)] public float WaitCost = 3f;
        [Min(0.1f)] public float DrawCost = 1f;
        [Min(0.1f)] public float ApproachCost = 2f;
        [Min(0.1f)] public float CloseInCost = 2f;
        [Min(0.1f)] public float RushCost = 1.5f;
        [Min(0.1f)] public float PunchCost = 1f;
        [Min(0.1f)] public float ChargedPunchCost = 2.5f;
        [Min(0.1f)] public float FetchPropCost = 2f;
        [Min(0.1f)] public float ShieldCost = 1.5f;
        [Min(0.1f)] public float ThrowCost = 3f;
        [Min(0.1f)] public float RetreatCost = 3f;
        [Min(0.1f)] public float ShieldedRetreatCost = 0.5f;
        [Min(0.1f)] public float KiteCost = 1f;
        [Min(0.1f)] public float RoamCost = 1f;
        [Min(0.1f)] public float ParryCost = 1f;
    }
}
