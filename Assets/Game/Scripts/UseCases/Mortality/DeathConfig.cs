using UnityEngine;

namespace Bw.UseCases.Mortality
{
    [CreateAssetMenu(fileName = "DeathConfig", menuName = "Configs/DeathConfig")]
    public sealed class DeathConfig : ScriptableObject
    {
        [Min(0)] public float CorpseTime = 3f;
        public LayerMask CorpseLayer = 1 << 2;
    }
}
