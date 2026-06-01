using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public readonly struct ShootRequestResultDto
    {
        public readonly ulong RequestId;
        public readonly Vector3 TargetPosition;
        public readonly bool Accepted;

        public ShootRequestResultDto(ulong requestId, Vector3 targetPosition, bool accepted)
        {
            RequestId = requestId;
            TargetPosition = targetPosition;
            Accepted = accepted;
        }
    }
}
