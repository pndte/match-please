using System;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Mortality;
using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.Tests.UseCases.Character.Doubles
{
    public sealed class FakeCharacter : IReadonlyCharacter
    {
        public IReadonlyMortal Mortal => throw new NotSupportedException("The fake character has only a place.");
        public IReadonlyMovement Movement => _movement;
        public IReadonlyHands Hands => throw new NotSupportedException("The fake character has only a place.");
        public IReadonlyWeaponInventory Inventory => throw new NotSupportedException("The fake character has only a place.");

        private readonly FakeMovement _movement = new();

        public FakeCharacter(Vector2 position)
        {
            _movement.Place(position);
        }

        public void MoveTo(Vector2 position) =>
            _movement.Place(position);
    }
}
