using UnityEngine;

namespace Bw.Entities
{
    public sealed class CursorPoint
    {
        public Vector2 World
        {
            get
            {
                var camera = _camera.Current.Value;
                var mouse = Input.mousePosition; //TODO: new input system
                mouse.z = camera.nearClipPlane;
                return camera.ScreenToWorldPoint(mouse);
            }
        }

        private readonly IChangableCamera _camera;

        public CursorPoint(IChangableCamera camera)
        {
            _camera = camera;
        }
    }
}
