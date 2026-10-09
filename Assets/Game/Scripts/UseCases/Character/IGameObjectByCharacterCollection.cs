using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.UseCases.Character
{
    public interface IGameObjectByCharacterCollection : IViewableMap<IReadonlyCharacter, GameObject>
    {
        
    }

    public class GameObjectByCharacterCollection : ViewableMap<IReadonlyCharacter, GameObject>, IGameObjectByCharacterCollection
    {
    }
}