using UnityEngine;

namespace Bw.UseCases.Combat.Shooting
{
    [CreateAssetMenu(fileName = "AmmoConfig", menuName = "Configs/AmmoConfig")]
    public class AmmoConfig : ScriptableObject
    {
        [Min(1)] public int Max;
        [Min(0)] public int OnSpawnValue;
    }
}