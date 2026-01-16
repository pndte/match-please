using System;
using JetBrains.Lifetimes;

namespace Entities
{
    public interface ILifetimed //TODO: переделать
    {
        public void WhenAlive(Action<Lifetime> action);
    }
}