using System;
using System.Collections.Generic;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Movement
{
    public sealed class Hands : IHands, IHeldItems
    {
        private readonly HoldOval _oval;
        private readonly List<IHeldItem> _items = new();

        public Hands(HoldOval oval)
        {
            _oval = oval;
        }

        public void Take(Lifetime lifetime, IHeldItem item)
        {
            if (_items.Contains(item))
                throw new InvalidOperationException("An item is taken into the hands once: it is already held by them.");

            _items.Add(item);
            lifetime.OnTermination(() => _items.Remove(item));
            item.Turned.Advise(lifetime, _ => item.PlaceIn(_oval.WorldRect()));
            item.PlaceIn(_oval.WorldRect());
        }

        public void Carry()
        {
            var oval = _oval.WorldRect();
            for (var index = 0; index < _items.Count; index++)
                _items[index].PlaceIn(oval);
        }
    }
}
