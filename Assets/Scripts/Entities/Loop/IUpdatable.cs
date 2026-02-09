using System;
using JetBrains.Lifetimes;

namespace Bw.Entities.Loop
{
    public interface IUpdatable
    {
        public void Update();
    }
    
    public interface ISubscrible 
    {
        void Subscribe(Lifetime lifetime, Action action);
    }
}