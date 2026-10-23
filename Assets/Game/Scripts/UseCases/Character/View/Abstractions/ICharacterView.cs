using Bw.UseCases.Mortality.View.Guarding;
using UnityEngine;

namespace Bw.UseCases.Character.View.Abstractions
{
    public interface ICharacterView
    {
        public ICharacterPoses Poses { get; }
        public IGripView GripView { get; }
        public IGuardViewModel Guard { get; }
        public Vector2 Position { get; }
    }
}
