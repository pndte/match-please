using JetBrains.Lifetimes;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Abstractions
{
    public interface IHands //TODO:очень странно, что у нас есть и руки,
                            //и инвентарь, это контринтуитивно, надо что-то думать
    
    //TODO: вообще надо подумать над API. API поднятия у пропа должно быть
    // простое и атомарное, типа Inventory.Take, Prop.Take, Hands.TakeProp и тд
    {
        public PhysicsBody Body { get; }
        public void Take(Lifetime lifetime, IHeldItem item);
    }
}
