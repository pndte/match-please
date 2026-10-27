using System;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class FakeHands : IReadonlyHands
    {
        public IReadonlyProperty<HandsState> State => _state;
        public HandsState Previous => HandsState.Empty;
        public ISource<HandsState> Stepped => _stepped;
        public bool Busy => false;
        public HoldPose Pose { get; set; }
        public GripPose Grip { get; set; }

        private readonly ViewableProperty<HandsState> _state = new(HandsState.Empty);
        private readonly Signal<HandsState> _stepped = new();

        public FakeHands(Vector2 position = default)
        {
            Pose = new HoldPose(position, new Rect(position - Vector2.one * 0.25f, Vector2.one * 0.5f));
        }

        public void MoveTo(Vector2 position) =>
            Pose = new HoldPose(position, new Rect(position - Vector2.one * 0.25f, Vector2.one * 0.5f));

        public bool TryFindInHands(out IWeapon weapon) =>
            throw new NotSupportedException("The fake hands do not know what they hold.");
    }
}
