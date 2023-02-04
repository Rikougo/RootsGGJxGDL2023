using UnityEngine;

namespace Roots.Utils
{
    static class VectorExtension
    {
        public static Vector2 XY(this Vector3 p_vector)
        {
            return new Vector2(p_vector.x, p_vector.x);
        }
    }
}