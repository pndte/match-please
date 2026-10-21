using Bw.Entities.Physics;
using Bw.UseCases.Movement;
using UnityEngine;

namespace Bw.UseCases.Bots.Navigation
{
    [CreateAssetMenu(fileName = "NavGraphConfig", menuName = "Configs/NavGraphConfig")]
    public sealed class NavGraphConfig : ScriptableObject
    {
        public BodyBox Character;
        public MovementConfig Movement;

        [Header("Nodes")]
        [Min(0.25f)] public float Spacing = 1f;

        [Header("Leaps")]
        public float[] JumpShares = { 0f, 0.35f, 0.7f, 1f };
        public int[] JumpHolds = { 1, 10, 40 };
        public int[] RunDelays = { 0, 8 };
        [Min(30)] public int MaxFlightTicks = 120;
    }
}
