using System;
using Unity.Netcode;

namespace Bw.Entities.Network
{
    public class NetworkHealthData : NetworkLifetimedBehaviour
    {
        [NonSerialized] public readonly NetworkVariable<float> Health = new();
    }
}