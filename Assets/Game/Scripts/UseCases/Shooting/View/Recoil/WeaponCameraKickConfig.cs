using UnityEngine;

namespace Bw.UseCases.Shooting.View.Recoil
{
    [CreateAssetMenu(fileName = "WeaponCameraKickConfig", menuName = "Configs/WeaponCameraKickConfig")]
    public class WeaponCameraKickConfig : ScriptableObject
    {
        [Min(0f)] public float Kick = 0.1f;
    }
}
