using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Character
{
    public interface IGameObjectByCharacterCollection : IViewableMap<IReadonlyMortal, GameObject>
    {
        
    }

    public class GameObjectByCharacterCollection : ViewableMap<IReadonlyMortal, GameObject>, IGameObjectByCharacterCollection
    {
    }
}