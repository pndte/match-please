using System.Collections.Generic;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.UseCases.PropsMatchBots.Doubles
{
    public sealed class StubHands : IHands
    {
        public IReadonlyProperty<HandsState> State => _state;
        public HandsState Previous => _state.Value;
        public ISource<HandsState> Stepped => _stepped;
        public bool Busy { get; set; }
        public HoldPose Pose { get; set; }
        public GripPose Grip { get; set; }
        public IWeapon InHands { get; set; }
        public bool Free { get; set; } = true;
        public IReadOnlyList<IWeapon> Taken => _taken;

        private readonly ViewableProperty<HandsState> _state = new(HandsState.Empty);
        private readonly Signal<HandsState> _stepped = new();
        private readonly List<IWeapon> _taken = new();

        public bool TryFindInHands(out IWeapon weapon)
        {
            weapon = InHands;
            return weapon != null;
        }

        public bool FreeFor(IWeapon item) =>
            Free;

        public void Take(IWeapon item) =>
            _taken.Add(item);

        public void Hold(HandsSlot inHands) =>
            _state.Value = new HandsState(inHands, HandsSlot.Empty, false);
    }
}
