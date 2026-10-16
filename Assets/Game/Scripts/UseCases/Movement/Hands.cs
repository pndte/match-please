using System;
using System.Collections.Generic;
using Bw.UseCases.Movement.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement
{
    public sealed class Hands : IHands, IHeldItems
    {
        public PhysicsBody Body { get; }

        private readonly Transform _holder;
        private readonly BoxCollider2D _shape;
        private readonly List<IHeldItem> _items = new();

        public Hands(Transform holder, BoxCollider2D shape, PhysicsBody body)
        {
            Body = body;
            _holder = holder;
            _shape = shape;
        }

        public void Take(Lifetime lifetime, IHeldItem item)
        {
            if (_items.Contains(item))
                throw new InvalidOperationException("An item is taken into the hands once: it is already held by them.");

            _items.Add(item);
            lifetime.OnTermination(() => _items.Remove(item));
            item.Turned.Advise(lifetime, _ => item.PlaceIn(Box()));
            item.PlaceIn(Box());
        }

        public void Carry()
        {
            var box = Box();
            for (var index = 0; index < _items.Count; index++)
                _items[index].PlaceIn(box);
        }

        private Rect Box()
        {
            var scale = (Vector2)_holder.lossyScale;
            var size = Vector2.Scale(_shape.size, scale);
            var center = (Vector2)_holder.position + Vector2.Scale(_shape.offset, scale);
            return new Rect(center - size / 2f, size);
        }
    }
}
