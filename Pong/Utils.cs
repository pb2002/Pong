using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pong
{
    public static class Utils
    {
        // Reflect vector in normal
        public static Vector2 Reflect(this Vector2 v, Vector2 normal)
        {
            // d' = d - 2(d . n) * n
            return v - 2 * Vector2.Dot(v, normal) * normal;
        }
        // Linear interpolation (lerp) for Vector2
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return a * t + b * (1 - t);
        }

        // TODO: Migrate to Collision.cs
        public static bool LineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersect)
        {
            // Sources:
            // https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection#Given_two_points_on_each_line
            // https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection#Given_two_points_on_each_line_segment
            intersect = Vector2.Zero;
            // calculating the determinant (D)
            Vector2 da = a2 - a1;
            Vector2 db = b2 - b1;
            float D = da.X * db.Y - da.Y * db.X;
            // D == 0 -> lines are parallel
            if (D == 0) return false;

            // Calculate t and u
            Vector2 c = b1 - a1;
            // c is equivalent to the (x1 - x3) and (y1 - y3) terms in the equations for t and u
            // D is equivalent to the denominator in the equations for t and u

            // so t will be:
            float t = (c.X * db.Y - c.Y * db.X) / D;
            // check if t is between 0 and 1 (otherwise intersection lies outside of line segments)
            if (t < 0 || t > 1) return false;

            // and u will be:
            float u = (c.X * da.Y - c.Y * da.X) / D;
            // idem
            if (u < 0 || u > 1) return false;

            // if this point is reached, a valid intersection point is found
            // we can now calculate the point using L1 or L2
            intersect = a1 + da * t;
            return true;
        }

        // TODO: Migrate to Collision.cs
        public static bool LineIntersection(Line a, Line b, out Vector2 intersect)
        {
            return LineIntersection(a.start, a.end, b.start, b.end, out intersect);
        }        

    }
}
