using UnityEngine;

namespace Bw.UseCases.Shooting.View
{
    [CreateAssetMenu(fileName = "ShotVfxConfig", menuName = "Configs/ShotVfxConfig")]
    public sealed class ShotVfxConfig : ScriptableObject
    {
        public GameObject Tracer;
        [Min(0.01f)] public float BulletSpeed = 80f;
    }
}
