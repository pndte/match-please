using UnityEngine;

namespace Bw.UseCases.Bots
{
    [CreateAssetMenu(fileName = "BotPersonality", menuName = "Configs/BotPersonality")]
    public sealed class BotPersonality : ScriptableObject
    {
        [Header("Goal weights (a goal's priority times its weight; a goal of weight 0 is left out)")]
        [Min(0f)] public float Kill = 1f;
        [Min(0f)] public float FinishOff = 1f;
        [Min(0f)] public float PunishReload = 1f;
        [Min(0f)] public float Guard = 1f;
        [Min(0f)] public float Retreat = 1f;
        [Min(0f)] public float Kite;

        [Header("Health shares")]
        [Range(0f, 1f)] public float RetreatHealth = 0.3f;
        [Range(0f, 1f)] public float WoundedHealth = 0.4f;

        [Header("Style")]
        [Min(0.5f)] public float CloseRange = 3.5f;
        [Min(0.1f)] public float MeleeCostScale = 1f;
        [Min(0.1f)] public float ShieldCostScale = 1f;
    }
}
