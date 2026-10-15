using UnityEngine;

namespace Bw.UseCases.Shooting.View
{
    [CreateAssetMenu(fileName = "FlashlightConfig", menuName = "Configs/FlashlightConfig")]
    public sealed class FlashlightConfig : ScriptableObject
    {
        public GameObject Beam;
    }
}
