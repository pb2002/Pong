using Microsoft.Xna.Framework;

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
            return a * (1 - t) + b * t;
        }

        // TODO: Migrate to Collision.cs
        public static bool LineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersect, out float t)
        {
            // Sources:
            // https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection#Given_two_points_on_each_line
            // https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection#Given_two_points_on_each_line_segment

            t = 0;
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
            t = (c.X * db.Y - c.Y * db.X) / D;
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
            return LineIntersection(a.start, a.end, b.start, b.end, out intersect, out float t);
        }
        public static bool LineIntersection(Line a, Line b, out Vector2 intersect, out float t)
        {
            return LineIntersection(a.start, a.end, b.start, b.end, out intersect, out t);
        }
        public static bool ClosestLineIntersection(Line[] a, Line[] b, out Vector2 intersect, out int ia, out int ib, out float t)
        {
            ia = -1;
            ib = -1;
            t = float.PositiveInfinity;
            intersect = Vector2.Zero;

            // Player edges are organized such that the first edge in the array is the one facing towards the playing field.
            // This edge is collided with the most,
            // so the outside for loop should iterate through the player edges for efficiency
            for (int j = 0; j < b.Length; j++) { 
                for (int i = 0; i < a.Length; i++)
                {
                    if(LineIntersection(a[i],b[j],out Vector2 _intersect, out float _t))
                    {
                        // minimize distance
                        // because all rays are the same length we can just compare the t value.
                        if (t > _t)
                        {
                            t = _t;
                            intersect = _intersect;
                            ia = i;
                            ib = j;
                        }
                    }
                }
            }
            return ia != -1;
        }
        public static Vector2 ResolveCollision(Vector2 start, Vector2 hit, Vector2 n, Vector2 delta)
        {
            float deltaLength = delta.Length();
            float startToHitLength = (start - hit).Length();

            // distance to travel after collision
            float remainingTravelDistance = deltaLength - startToHitLength;

            // calculate reflected travel direction
            Vector2 dn = Vector2.Normalize(delta);
            Vector2 rdn = dn.Reflect(n);

            // final position
            return rdn * remainingTravelDistance;
        }

        public static bool CheckBoxCollision(Box a, Box b, Vector2 delta, out Collision collision)
        {
            collision = new Collision();
            Line[] rays = a.GetCollisionRays(delta);
            
            if (!Utils.ClosestLineIntersection(rays, b.Edges, out Vector2 intersect, 
                out int ia, out int ib, out float t)) return false;
            
            collision.hitEdgeIndex = ib;
            collision.intersection = intersect;
            collision.corner = rays[ia].start;
            collision.hit = intersect + a.position - collision.corner;
            return true;
        }
    }
}
