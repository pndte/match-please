using UnityEngine;

namespace Bw.UseCases.Shooting.View.Recoil
{
    [CreateAssetMenu(fileName = "WeaponRecoilConfig", menuName = "Configs/WeaponRecoilConfig")]
    public sealed class WeaponRecoilConfig : ScriptableObject
    {
        [Header("Kick (units back along the barrel)")]
        public RecoilPunch Kick = new() { Amount = 0.12f, OutTime = 0.02f, ReturnTime = 0.07f, Overshoot = 1.95f };

        [Header("Tilt (degrees the barrel tips up)")]
        public RecoilPunch Tilt = new() { Amount = 5f, OutTime = 0.025f, ReturnTime = 0.095f, Overshoot = 2.3f };
    }
}
