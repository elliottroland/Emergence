/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using Emergence.Map;

namespace Emergence {
    class Velocities {
        public Vector3 persistentVelocity = Vector3.Zero,
            momentumVelocity = Vector3.Zero;
    }

    public class PhysicsEngine {
        CoreEngine core;
        Velocities[] playerVelocities = { new Velocities(), new Velocities(), new Velocities(), new Velocities() };

        public float gravity = 50f;   //units per second

        public PhysicsEngine(CoreEngine c) {
            core = c;
        }

        private Vector3 size(BoundingBox b) {
            return b.Max - b.Min;
        }

        private Vector3 center(BoundingBox b) {
            return b.Min + size(b) * 0.5f;
        }

        class LineSegment {
            public Vector3 a, b;
            public LineSegment(Vector3 a, Vector3 b) {
                this.a = a;
                this.b = b;
            }
        }

        class PushOutComparer : IComparer<Vector3> {
            Vector3 move;
            public PushOutComparer(Vector3 m) {
                move = Vector3.Normalize(m);
            }

            int IComparer<Vector3>.Compare(Vector3 a, Vector3 b) {
                float aDot = (Math.Abs(Vector3.Dot(Vector3.Normalize(a), move))),
                    bDot = (Math.Abs(Vector3.Dot(Vector3.Normalize(b), move)));
                if (aDot > 0)
                    return (bDot > 0 ? 0 : 1);
                if (bDot > 0)
                    return (aDot > 0 ? 0 : -1);

                float aSize = Vector3.Dot(a, a),
                      bSize = Vector3.Dot(b, b);
                if (aSize < bSize)
                    return -1;
                else if (aSize > bSize)
                    return 1;
                return 0;
            }
        }


        public Vector3 getPushOut(BoundingBox mover, Brush brush, Vector3 move) {
            //The mover has moved to a newposition
            BoundingBox newPosition = new BoundingBox(mover.Min + move, mover.Max + move);

            /*--------------------------------------------------
             * Collision detection
             *--------------------------------------------------*/

            /* Three lines are used to determine collision with a plane.
             * Each goes from a bottom corner of the bounding box to the
             * opposite, upper corner
             * /
            Vector3 minMaxMove = newPosition.Max - newPosition.Min;
            List<LineSegment> segments = new List<LineSegment>();
            segments.Add(new LineSegment(newPosition.Min, newPosition.Max));
            segments.Add(new LineSegment(newPosition.Min + new Vector3(minMaxMove.X, 0, 0), newPosition.Max - new Vector3(minMaxMove.X, 0, 0)));
            segments.Add(new LineSegment(newPosition.Min + new Vector3(0, 0, minMaxMove.Z), newPosition.Max - new Vector3(0, 0, minMaxMove.Z)));

            /* Now we check which of these line segments collide with the faces
             * and for each line-face collision we calculate a pushout vector in
             * the direction of the normal of the face
             * /
            List<Vector3> pushouts = new List<Vector3>();
            foreach (Face f in brush.faces) {
                Vector3 norm = f.plane.getNormal();
                float d = f.plane.getD();
                List<Vertex> verts = f.vertices;
                foreach(LineSegment l in segments)  {
                    /* Each line can be represented by the equation <x,y,z> = <p1,p2,p3> + t<a,b,c>
                     * where <p1,p2,p3> is a point on the line and <a,b,c> is the direction vector.
                     * Now we can take <a,b,c> = <q1-p1,q2-p2,q3-p3> where q and p are the points in
                     * the line segment, and then solve for the t that represents the collision point
                     * of the line and the face's plane. If 0 <= t < 1 then the point is on the line segment
                     * and all that's left to check is whether the point is on the face (and not just the
                     * plane). We *could* use the center of the face and do dot-products to calculate this
                     * but we'll need the intersection point later if there's a collision, so it's worth
                     * getting it now
                     * /
                    Vector3 dir = l.b - l.a;
                    float denom = Vector3.Dot(norm, dir);
                    if (Math.Abs(denom) <= float.Epsilon)   continue;   //here's it's effectively zero
                    float t = -(d + Vector3.Dot(norm, l.a)) / denom;
                    //if 0 <= t < 1 then this line segment collides with our plane
                    if (t < 1 && t > 0) {
                        //the point, on the plane, where the line segment intersects it
                        Vector3 intersection = l.a + t*dir;
                        /* To check whether the point is on the face we check, for each line segment that
                         * makes up our face, if the cross-product always points in the same direction. If
                         * not, then the point lies outside of the face
                         * /
                        //get the reference line
                        bool inFace = true;
                        Vector3 cross = Vector3.Cross(verts[1].position - verts[0].position, intersection - verts[0].position);
                        for (int i = 1; i < verts.Count + 1; i++) {
                            Vector3 c = Vector3.Cross(verts[i % verts.Count].position - verts[i-1].position, intersection - verts[i-1].position);
                            if ((cross.X != 0 && c.X / cross.X < 0) || (cross.Y != 0 && c.Y / cross.Y < 0) || (cross.Z != 0 && c.Z / cross.Z < 0)) {
                                inFace = false;
                                break;
                            }
                        }
                        if (inFace) {
                            /* If we're here then the intersection point is on the face. Now we can calculate
                             * the distance this point is from the face, in the direction of its normal. We
                             * can't really use t, though. We also need to figure out which of the points
                             * is behind the face and which is in front.
                             * /
                            Vector3 pointBehind = (Vector3.Dot(norm, l.a-f.getCenter()) > 0 ? l.b : l.a);
                            /* The direction vector of the shortest line from pointBehind to the face
                             * is the normal of the face. So we can use the same solution as last time
                             * to figure out the intersection point
                             * /
                            denom = Vector3.Dot(norm, norm);
                            //we know that denom won't be equal to zero, so there's no need to check it
                            t = -(d + Vector3.Dot(norm, pointBehind)) / denom;
                            intersection = pointBehind + t * norm;

                            //now for the push out, we take intersection-pointBehind
                            pushouts.Add(intersection - pointBehind);
                            //Console.WriteLine("collision");
                        }
                    }
                }
            }
            if (pushouts.Count == 0)
                return Vector3.Zero;

            /*--------------------------------------------------
             * Collision response
             *--------------------------------------------------* /

            /* Now that we have a list of plausible pushouts, we take the minimum one.
             * If there is a tie, we take the one that has a dot product closest to zero
             * with the move vector
             * /
            pushouts.Sort(new PushOutComparer(move));
            return pushouts[0];
        }

        public bool onFloor(GameTime gameTime, PlayerIndex pi) {
            BoundingBox playerBoundingBox = core.players[(int)pi].getBoundingBox();
            Vector3 gravity = new Vector3(0, -(float)gameTime.ElapsedGameTime.TotalSeconds * this.gravity, 0);
            //Vector3 gravity = getGravity(gameTime, pi) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (Brush b in core.mapEngine.brushes) {
                if (playerBoundingBox.Intersects(b.boundingBox)) {
                    Vector3 po = getPushOut(core.players[(int)pi].getBoundingBox(), b, gravity);
                    if (po != Vector3.Zero)
                        gravity = gravity + po;
                    if (gravity.Y >= 0)
                        break;
                }
            }
            return gravity.Y >= -0.05;
        }

        public Vector3 applyCollision(GameTime gameTime, PlayerIndex pi, Vector3 velocity) {
            Player player = core.players[(int)pi];
            BoundingBox playerBoundingBox = player.getBoundingBox();
            /*Velocities pv = playerVelocities[(int)pi];
            pv.persistentVelocity.Y = Math.Max(pv.persistentVelocity.Y - core.physicsEngine.gravity, player.terminalVelocity);
            bool playerOnFloor = onFloor(gameTime, pi);
            Console.WriteLine("player on floor: " + playerOnFloor);
            if (playerOnFloor) {
                pv.persistentVelocity.Y = velocity.Y;
                pv.momentumVelocity = Vector3.Zero;
            }* /
            //velocity.Y = 0; //we've extracted this effect in the above if-statement

            Vector3 retMove = (velocity /*+ pv.persistentVelocity*/) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 originalRetMove = new Vector3(retMove.X, retMove.Y, retMove.Z);
            Vector3 p = Vector3.Zero;
            foreach (Brush b in core.mapEngine.brushes) {
                b.colliding = false;
                if (playerBoundingBox.Intersects(b.boundingBox)) {
                    Vector3 po = getPushOut(playerBoundingBox, b, retMove);
                    if (po != Vector3.Zero) {
                        b.colliding = true;
                        retMove = retMove + po;
                    }
                }
            }

            /*if (retMove.Y != originalRetMove.Y)
                pv.persistentVelocity.Y = 0; */

            return retMove;
        }
    }
}
*/