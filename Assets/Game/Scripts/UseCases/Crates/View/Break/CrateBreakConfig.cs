using System;
using UnityEngine;

namespace Bw.UseCases.Crates.View.Break
{
    [Serializable]
    public sealed class CrateBreakConfig
    {
        public GameObject Debris;
        public Vector2 DebrisOffset = new(0f, 0.5f);
    }
}
