using Bw.UseCases.Mortality;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IShotPower<TState> where TState : struct
    {
        public Hit Along(Vector2 direction, TState state);
    }
}
