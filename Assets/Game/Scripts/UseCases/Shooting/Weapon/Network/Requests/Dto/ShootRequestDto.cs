using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public readonly struct ShootRequestDto
    {
        public readonly ulong RequestId;
        public readonly Vector3 TargetPosition;

        public ShootRequestDto(ulong requestId, Vector3 targetPosition)
        {
            RequestId = requestId;
            TargetPosition = targetPosition;
        }
    }
}