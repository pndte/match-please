using System;
using System.Collections.Generic;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Mortality.Guarding
{
    public sealed class GuardCovers : IGuardCovers
    {
        private static readonly IGuardCover Bare = new BareCover();

        private readonly Dictionary<IReadonlyMortal, IGuardCover> _covers = new();

        public IGuardCover CoverOf(IReadonlyMortal mortal) =>
            _covers.TryGetValue(mortal, out var cover) ? cover : Bare;

        public void AddLifetimed(Lifetime lifetime, IReadonlyMortal mortal, IGuardCover cover)
        {
            if (!_covers.TryAdd(mortal, cover))
                throw new InvalidOperationException("A mortal has one guard at a time: its cover was added again before the last one ended.");

            lifetime.OnTermination(() => _covers.Remove(mortal));
        }

        private sealed class BareCover : IGuardCover
        {
            public bool InCover(Contact contact) =>
                false;
        }
    }
}
