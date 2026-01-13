using System;
using Unity.Netcode;

namespace Entities.Network
{
    public class NetworkHealthData : NetworkLifetimedBehaviour
    {
        [NonSerialized] public readonly NetworkVariable<float> Health = new();
    }
}