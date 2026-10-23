using Bw.UseCases.Mortality;
using UnityEngine;

namespace Bw.UseCases.Combat.Weapon.Abstractions
{
    public interface IShotPower<TState> where TState : struct
    {
        public Hit Along(Vector2 direction, TState state);
    }
}
