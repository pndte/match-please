using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Level
{
    public sealed class Springboard : MonoBehaviour, ISpringboard
    {
        public float LaunchSpeed => _launchSpeed;

        [SerializeField, Min(0f)] private float _launchSpeed = 40f;
    }
}
