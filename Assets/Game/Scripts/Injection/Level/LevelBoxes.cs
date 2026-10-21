using System.Collections.Generic;
using Bw.Entities.Physics;
using UnityEngine.SceneManagement;
using Zenject;

namespace Bw.Injection.Level
{
    public static class LevelBoxes
    {
        public static BodyBox[] In(Scene scene)
        {
            var boxes = new List<BodyBox>();
            foreach (var root in scene.GetRootGameObjects())
                foreach (var box in root.GetComponentsInChildren<BodyBox>(false))
                    if (box.GetComponentInParent<GameObjectContext>(true) == null)
                        boxes.Add(box);

            return boxes.ToArray();
        }
    }
}
