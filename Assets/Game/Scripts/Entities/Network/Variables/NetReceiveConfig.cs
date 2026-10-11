using System;
using UnityEngine;

namespace Bw.Entities.Network.Variables
{
    [Serializable]
    public sealed class NetReceiveConfig
    {
        [Min(0f)] public float StaleWriterSeconds = 1f;
    }
}
