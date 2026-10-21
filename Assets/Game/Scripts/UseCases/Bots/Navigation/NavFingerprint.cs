using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using UnityEngine;

namespace Bw.UseCases.Bots.Navigation
{
    public static class NavFingerprint
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        public static string Of(IReadOnlyList<BodyBox> level, NavGraphConfig config, float step)
        {
            var boxes = new List<string>(level.Count);
            foreach (var box in level)
                boxes.Add(Describe(box));

            boxes.Sort(string.CompareOrdinal);
            var movement = config.Movement;
            var text = new StringBuilder();
            Append(text, step, Physics2D.gravity.y, movement.Speed, movement.JumpForce, movement.GravityScale, movement.CoyoteTime, movement.JumpBufferTime, movement.ReleasedJumpGravity, movement.FallGravity);
            text.Append(movement.GroundLayer.value.ToString(CultureInfo.InvariantCulture)).Append('|');
            Append(text, config.Character.WorldSize().x, config.Character.WorldSize().y, config.Character.WorldOffset().x, config.Character.WorldOffset().y, config.Spacing, config.MaxFlightTicks);
            Append(text, config.JumpShares);
            foreach (var hold in config.JumpHolds)
                text.Append(hold.ToString(CultureInfo.InvariantCulture)).Append(',');

            foreach (var delay in config.RunDelays)
                text.Append(delay.ToString(CultureInfo.InvariantCulture)).Append(',');

            foreach (var box in boxes)
                text.Append(box).Append(';');

            return Hash(text.ToString());
        }

        private static string Describe(BodyBox box)
        {
            var geometry = box.WorldGeometry();
            var text = new StringBuilder(box.IsOneWay ? "o" : "s");
            for (var vertex = 0; vertex < geometry.count; vertex++)
                Append(text, geometry.vertices[vertex].x, geometry.vertices[vertex].y);

            return text.ToString();
        }

        private static void Append(StringBuilder text, params float[] values)
        {
            foreach (var value in values)
                text.Append(value.ToString("0.###", CultureInfo.InvariantCulture)).Append(',');

            text.Append('|');
        }

        private static string Hash(string text)
        {
            var hash = OffsetBasis;
            foreach (var symbol in Encoding.UTF8.GetBytes(text))
            {
                hash ^= symbol;
                hash *= Prime;
            }

            return hash.ToString("x16", CultureInfo.InvariantCulture);
        }
    }
}
