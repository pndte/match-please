using Bw.UseCases.Props;
using UnityEngine;

namespace Bw.Injection.Props
{
    public sealed class PropParts
    {
        public Transform Prop { get; }
        public Transform Visual { get; }
        public BoxCollider2D Shape { get; }
        public HeldPropConfig Held { get; }
        public PropThrowConfig Throw { get; }
        public int HeldSortingOrder { get; }

        public PropParts(
            Transform prop,
            Transform visual,
            BoxCollider2D shape,
            HeldPropConfig held,
            PropThrowConfig @throw,
            int heldSortingOrder)
        {
            Prop = prop;
            Visual = visual;
            Shape = shape;
            Held = held;
            Throw = @throw;
            HeldSortingOrder = heldSortingOrder;
        }
    }
}
