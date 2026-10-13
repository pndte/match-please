using System;
using UnityEngine;

namespace Bw.Injection
{
    public static class LayerMaskExtensions
    {
        public static int SingleLayer(this LayerMask mask, string role)
        {
            for (var layer = 0; layer < 32; layer++)
                if (mask.value == 1 << layer)
                    return layer;

            throw new ArgumentException($"The {role} layer must be exactly one layer, the mask is {mask.value}.", nameof(mask));
        }
    }
}
