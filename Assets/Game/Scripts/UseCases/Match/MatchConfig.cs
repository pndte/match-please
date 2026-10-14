using UnityEngine;

namespace Bw.UseCases.Match
{
    [CreateAssetMenu(fileName = "MatchConfig", menuName = "Configs/MatchConfig")]
    public sealed class MatchConfig : ScriptableObject
    {
        [Min(1f)] public float MatchSeconds = 600f;
        [Min(1f)] public float ResultSeconds = 10f;
    }
}
