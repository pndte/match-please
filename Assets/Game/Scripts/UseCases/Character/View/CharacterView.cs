using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Mortality.View.Guarding;
using UnityEngine;

namespace Bw.UseCases.Character.View
{
    public sealed class CharacterView : ICharacterView
    {
        public ICharacterPoses Poses { get; }
        public IGripView GripView { get; }
        public IGuardViewModel Guard { get; }
        public Vector2 Position => _root.position;

        private readonly Transform _root;

        public CharacterView(
            ICharacterPoses poses,
            IGripView gripView,
            IGuardViewModel guard,
            Transform root)
        {
            Poses = poses;
            GripView = gripView;
            Guard = guard;
            _root = root;
        }
    }
}
