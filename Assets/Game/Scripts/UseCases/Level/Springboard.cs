using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Level
{
    public sealed class Springboard : MonoBehaviour, ISpringboard
    {
        public float LaunchHeight => _launchHeight;

        [SerializeField, Min(0f)] private float _launchHeight = 12.93f;
    }
}
