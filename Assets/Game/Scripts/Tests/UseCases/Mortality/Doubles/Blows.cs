using Bw.UseCases.Mortality;
using UnityEngine;

namespace Bw.Tests.UseCases.Mortality.Doubles
{
    public static class Blows
    {
        public static Impact At(float x, float y, float damage) =>
            new(new Hit(damage, Vector2.zero), new Contact(new Vector2(x, y)));

        public static Impact Pushing(Vector2 knockback, Vector2 point, float damage = 10f) =>
            new(new Hit(damage, knockback), new Contact(point));

        public static Impacts Of(params Impact[] impacts) =>
            new(impacts);
    }
}
