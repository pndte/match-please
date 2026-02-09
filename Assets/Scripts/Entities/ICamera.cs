using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.Entities
{
    public interface IChangableCamera
    {
        public IReadonlyProperty<Camera> Current { get; }
    }

    public class PlayerCamera : IChangableCamera
    {
        public IReadonlyProperty<Camera> Current => _camera;
        private readonly ViewableProperty<Camera> _camera = new();
        public PlayerCamera(Camera camera)
        {
            _camera.Value = camera;
        }
    }
}