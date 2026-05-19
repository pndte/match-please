using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Character
{
    public interface IGameObjectByCharacterCollection : IViewableMap<ICharacter, GameObject>
    {
        
    }

    public class GameObjectByCharacterCollection : ViewableMap<ICharacter, GameObject>, IGameObjectByCharacterCollection
    {
    }
}