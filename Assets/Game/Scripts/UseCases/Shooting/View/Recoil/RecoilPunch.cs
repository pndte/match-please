using System;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Recoil
{
    [Serializable]
    public sealed class RecoilPunch
    {
        public float Amount;
        [Min(0.005f)] public float OutTime = 0.02f;
        [Min(0.01f)] public float ReturnTime = 0.08f;
        [Min(0f)] public float Overshoot = 1.7f;
    }
}
