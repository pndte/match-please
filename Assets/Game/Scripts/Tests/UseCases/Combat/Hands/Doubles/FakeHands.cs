using System.Collections.Generic;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Hands.Doubles
{
    public sealed class FakeHands : IHands
    {
        public IReadonlyProperty<HandsState> State => _state;
        public HandsState Previous => _state.Value;
        public ISource<HandsState> Stepped => _stepped;
        public bool Busy => false;
        public HoldPose Pose { get; set; } = new(Vector2.zero, new Rect(-0.5f, -1f, 1f, 2f));
        public GripPose Grip => default;
        public bool Free { get; set; } = true;
        public IReadOnlyList<IWeapon> Taken => _taken;

        private readonly ViewableProperty<HandsState> _state = new(HandsState.Empty);
        private readonly Signal<HandsState> _stepped = new();
        private readonly List<IWeapon> _taken = new();

        public bool TryFindInHands(out IWeapon weapon)
        {
            weapon = null;
            return false;
        }

        public bool FreeFor(IWeapon item) =>
            Free;

        public void Take(IWeapon item) =>
            _taken.Add(item);
    }
}
