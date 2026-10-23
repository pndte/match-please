using UnityEngine;

namespace Bw.UseCases.Combat.View
{
    [CreateAssetMenu(fileName = "FlashlightConfig", menuName = "Configs/FlashlightConfig")]
    public sealed class FlashlightConfig : ScriptableObject
    {
        public GameObject Beam;
    }
}
