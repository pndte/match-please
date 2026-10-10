using Bw.UseCases.Shooting.View.Crosshair.Abstractions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bw.UseCases.Shooting.View.Crosshair
{
    public sealed class SystemCursor : ISystemCursor
    {
        private Vector2 _moved;
        private int _movedFrame = -1;

        public Vector2 Position =>
            _movedFrame == Time.frameCount ? _moved : (Vector2)Input.mousePosition; //TODO: new input system

        public void MoveTo(Vector2 position) //TODO: движение мыши, сделанное в кадре переноса до него, теряется: новую позицию считают от позиции начала кадра
        {
            _moved = new Vector2(Mathf.Clamp(position.x, 0f, Screen.width - 1f), Mathf.Clamp(position.y, 0f, Screen.height - 1f));
            _movedFrame = Time.frameCount;
            Mouse.current.WarpCursorPosition(_moved);
        }
    }
}
