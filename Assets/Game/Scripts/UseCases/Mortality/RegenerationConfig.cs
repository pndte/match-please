using UnityEngine;

namespace Bw.UseCases.Mortality
{
    [CreateAssetMenu(fileName = "RegenerationConfig", menuName = "Configs/RegenerationConfig")]
    public sealed class RegenerationConfig : ScriptableObject
    {
        [Min(0)] public float Delay = 4f;
        [Min(0.05f)] public float Interval = 0.5f;
        [Min(0.1f)] public float Amount = 10f;
    }
}
