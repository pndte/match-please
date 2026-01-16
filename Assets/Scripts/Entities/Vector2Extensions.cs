using UnityEngine;

namespace Entities
{
    public static class Vector2Extensions
    {
        public static Vector2 WithX(this Vector2 vector, float x)
        {
            return new Vector2(x, vector.y);
        }
        
        public static Vector2 WithY(this Vector2 vector, float y)
        {
            return new Vector2(vector.x, y);
        }
    }
}