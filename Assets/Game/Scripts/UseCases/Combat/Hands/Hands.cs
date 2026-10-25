using System;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands.Extensions;
using Bw.UseCases.Combat.Weapon.Extensions;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Combat.Hands
{
    public sealed class Hands : IHands, IRestorable<HandsState>, ISteppable<HandsInput>
    {
        public IReadonlyProperty<HandsState> State => _state;
        public HandsState Previous { get; private set; } = HandsState.Empty;
        public ISource<HandsState> Stepped => _stepped;
        public bool Busy => TryFindInHands(out var inHands) && !inHands.Stowable;
        public HoldPose Pose => _oval.PoseAt(_oval.transform.position); //TODO: на клиенте поза рук (хват, дуло, места выпуска) считается от нарисованного корня — между тиками и со сдвигом сглаживания, — а на сервере от просчитанного: игровые позы брать из просчитанного, а картинку оружия оставить за нарисованным
        public GripPose Grip => _grip.Pose;

        private readonly ViewableProperty<HandsState> _state = new(HandsState.Empty);
        private readonly Signal<HandsState> _stepped = new();
        private readonly ISimulator<HandsInput, HandsState> _simulator;
        private readonly HandsSimulator _rules;
        private readonly IWeaponInventory _inventory;
        private readonly HoldOval _oval;
        private readonly HandsGrip _grip;

        public Hands(
            ISimulator<HandsInput, HandsState> simulator,
            HandsSimulator rules,
            IWeaponInventory inventory,
            HoldOval oval,
            HandsGrip grip)
        {
            _simulator = simulator;
            _rules = rules;
            _inventory = inventory;
            _oval = oval;
            _grip = grip;
        }

        public bool TryFindInHands(out IWeapon weapon) =>
            _inventory.TryFindTop(_state.Value.InHands, out weapon);

        public bool FreeFor(IWeapon item) =>
            !Busy && !(_inventory.RulesOf(item).HandsOnly && _state.Value.TakenByGuard);

        public void Take(IWeapon item)
        {
            if (!FreeFor(item))
                throw new InvalidOperationException("The hands take an item only while they are free for it: ask FreeFor first.");

            _inventory.PickUpSwapping(item);
            Apply(_rules.Taken(_state.Value, _inventory.Carried[item]));
        }

        public void Apply(HandsState state) =>
            _state.Value = state;

        public void Step(HandsInput input)
        {
            Previous = _state.Value;
            _state.Value = _simulator.Step(Previous, input);
            _stepped.Fire(_state.Value);
            if (TryFindInHands(out var inHands))
                inHands.Step(input.Weapon);
        }
    }
}
