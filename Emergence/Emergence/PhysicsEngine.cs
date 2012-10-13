using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using Emergence.Map;

namespace Emergence {
    public class Velocities {
        public Vector3 persistentVelocity = Vector3.Zero,
            momentumVelocity = Vector3.Zero;
    }

    public class PhysicsEngine {
        CoreEngine core;
        //Velocities[] playerVelocities = { new Velocities(), new Velocities(), new Velocities(), new Velocities() };

        public float gravity = 17f;   //units per second

        public PhysicsEngine(CoreEngine c) {
            core = c;
        }

        private Vector3 size(BoundingBox b) {
            return b.Max - b.Min;
        }

        private Vector3 center(BoundingBox b) {
            return b.Min + size(b) * 0.5f;
        }

        public Vector3 toESpace(Vector3 radius, Vector3 toTransform) {
            return new Vector3(toTransform.X / radius.X, toTransform.Y / radius.Y, toTransform.Z / radius.Z);
        }

        public Vector3 toWorldSpace(Vector3 radius, Vector3 toTransform) {
            return new Vector3(toTransform.X * radius.X, toTransform.Y * radius.Y, toTransform.Z * radius.Z);
        }

        public Vector3 getPushOut(BoundingBox mover, Brush brush, Vector3 move) {
            return Vector3.Zero;
        }

        public float signedDistance(Vector3 N, Vector3 p, float D) {
            return Vector3.Dot(N, p) + D;
        }

        public class CollisionPackage {
            public Vector3 position, velocity, closestCollisionPoint;
            public float closestCollisionDistance;
            public bool collision = false;
            public CollisionPackage(Vector3 p, Vector3 v, bool c, Vector3 ccp, float ccd) {
                position = p;
                velocity = v;
                collision = c;
                closestCollisionDistance = ccd;
                closestCollisionPoint = ccp;
            }
        }

        public CollisionPackage collides(Vector3 moverRadius, Vector3 basePoint, Vector3 velocity) {
            float closestCollisionTime = 1;
            Vector3 closestCollisionPoint = Vector3.Zero;
            foreach(Brush brush in core.mapEngine.brushes)    {
                foreach (Face face in brush.faces) {
                    face.DiffuseColor = new Vector3(1, 1, 1);
                    //we want the vertices of this face converted into eSpace
                    List<Vector3> eSpaceVerts = new List<Vector3>();
                    foreach (Vertex vert in face.vertices)
                        eSpaceVerts.Add(toESpace(moverRadius, vert.position));

                    //find the normal of the face's plane in eSpace
                    Microsoft.Xna.Framework.Plane helperPlane = new Microsoft.Xna.Framework.Plane(toESpace(moverRadius, face.plane.first), toESpace(moverRadius, face.plane.second), toESpace(moverRadius, face.plane.third));
                    helperPlane.Normal = -helperPlane.Normal;
                    Vector3 N = helperPlane.Normal;
                    N.Normalize();
                    float D = -helperPlane.D;

                    if (Vector3.Dot(N, Vector3.Normalize(velocity)) >= -0.005)
                        continue;

                    float t0, t1;
                    bool embeddedInPlane = false;
                    if (Vector3.Dot(N, velocity) != 0) {
                        t0 = (-1 - signedDistance(N, basePoint, D)) / Vector3.Dot(N, velocity);
                        t1 = ( 1 - signedDistance(N, basePoint, D)) / Vector3.Dot(N, velocity);
                        //swap them so that t0 is smallest
                        if (t0 > t1) {
                            float temp = t1;
                            t1 = t0;
                            t0 = temp;
                        }

                        if (t0 < 0 && t1 > 1)
                            face.DiffuseColor = new Vector3(0, 1, 0);

                        if (t0 > 1 || t1 < 0)
                            continue;
                    }
                    else if (Math.Abs(signedDistance(N, basePoint, D)) < 1) {
                        t0 = 0;
                        t1 = 1;
                        embeddedInPlane = true;
                    }
                    else  //in this case we can't collide with this face
                        continue;
                    
                    if (!embeddedInPlane) {
                        //now we find the plane intersection point
                        //Vector3 planeIntersectionPoint = basePoint - N + t0 * velocity;
                        Vector3 planeIntersectionPoint = basePoint - N + t0 * velocity;

                        //project this onto the plane

                        //clamp [t0, t1] to [0,1]
                        if (t0 < 0) t0 = 0;
                        if (t1 < 0) t1 = 0;
                        if (t0 > 1) t0 = 1;
                        if (t1 > 1) t1 = 1;

                        //now we find if this point lies on the face

                        if (MapEngine.pointOnFace(planeIntersectionPoint, eSpaceVerts, N)) {
                            //float intersectionDistance = t0 * (float)Math.Sqrt(Vector3.Dot(velocity, velocity));
                            //NEED TO DO SOMETHING HERE--------------------------------------------------------------------------
                            if(face.DiffuseColor == new Vector3(1,1,1))
                                face.DiffuseColor = new Vector3(1, 0, 0);
                            if (t0 < closestCollisionTime) {
                                closestCollisionTime = t0;
                                closestCollisionPoint = planeIntersectionPoint;
                            }
                            //return true;
                        }
                    }
                    {  //do the "sweep test"
                        //sweep against the vertices
                        foreach (Vector3 p in eSpaceVerts) {
                            //calculate A, B and C that make up our quadratic equation
                            //  At^2 + Bt + C = 0
                            float A = Vector3.Dot(velocity, velocity),
                                B = 2 * Vector3.Dot(velocity, basePoint - p),
                                C = Vector3.Dot(p - basePoint, p - basePoint) - 1;

                            //check that the equation has a solution
                            float discriminant = B * B - 4 * A * C;

                            //in this case we have a solution to the equation
                            if (discriminant >= 0) {
                                float r1 = (-B + (float)Math.Sqrt(discriminant)) / (2 * A),
                                    r2 = (-B - (float)Math.Sqrt(discriminant)) / (2 * A);
                                /*float x1 = Math.Min(r1, r2), x2 = Math.Max(r1, r2);
                                if (x1 < 0)
                                    x1 = x2;*/
                                if (r1 > r2) {
                                    float temp = r2;
                                    r2 = r1;
                                    r1 = temp;
                                }
                                float root = -1;
                                if (r1 > 0 && r1 < closestCollisionTime)
                                    root = r1;
                                else if (r2 > 0 && r2 < closestCollisionTime)
                                    root= r2;
                                //NEED TO DO SOMETHING HERE--------------------------------------------------------------------------
                                /* intersectionPoint = p;
                                 * intersectionDistance = x1 * (float)Math.Sqrt(Vector3.Dot( velocity, velocity ));
                                 */
                                if (root != -1) {
                                    closestCollisionTime = root;
                                    closestCollisionPoint = p;
                                }
                            }
                        }

                        //sweep against the edges
                        for (int i = 0; i < eSpaceVerts.Count; i++) {
                            //calculate A, B and C that make up our quadratic equation
                            //  At^2 + Bt + C = 0
                            Vector3 p1 = eSpaceVerts[i],
                                    p2 = eSpaceVerts[(i + 1) % eSpaceVerts.Count];
                            Vector3 edge = p2 - p1;
                            Vector3 baseToVertex = p1 - basePoint;
                            float A = Vector3.Dot(edge, edge) * (-Vector3.Dot(velocity, velocity)) + (float)Math.Pow(Vector3.Dot(edge, velocity), 2),
                                B = Vector3.Dot(edge, edge) * 2 * Vector3.Dot(velocity, baseToVertex) - 2 * (Vector3.Dot(edge, velocity) * Vector3.Dot(edge, baseToVertex)),
                                C = Vector3.Dot(edge, edge) * (1 - Vector3.Dot(baseToVertex, baseToVertex)) + (float)Math.Pow(Vector3.Dot(edge, baseToVertex), 2);

                            //check that the equation has a solution
                            float discriminant = B * B - 4 * A * C;

                            //in this case we have a solution to the equation
                            if (discriminant >= 0) {
                                float r1 = (-B + (float)Math.Sqrt(discriminant)) / (2 * A),
                                    r2 = (-B - (float)Math.Sqrt(discriminant)) / (2 * A);
                                if (r1 > r2) {
                                    float temp = r2;
                                    r2 = r1;
                                    r1 = temp;
                                }
                                float root = -1;
                                if (r1 > 0 && r1 < closestCollisionTime)
                                    root = r1;
                                else if (r2 > 0 && r2 < closestCollisionTime)
                                    root = r2;
                                if (root != -1) {
                                    float f0 = (Vector3.Dot(edge, velocity) * root - Vector3.Dot(edge, baseToVertex)) / (Vector3.Dot(edge, edge));
                                    //NEED TO DO SOMETHING HERE--------------------------------------------------------------------------
                                    /* intersectionPoint = p1 + f0 * edge;
                                     * intersectionDistance = x1 * (float)Math.Sqrt(Vector3.Dot( velocity, velocity ));
                                     */
                                    if (0 <= f0 && f0 <= 1) {
                                        closestCollisionTime = root;
                                        closestCollisionPoint = p1 + f0 * edge;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return new CollisionPackage(basePoint, velocity, closestCollisionTime < 1, closestCollisionPoint, closestCollisionTime * (float)Math.Sqrt(Vector3.Dot(velocity, velocity)));
        }

        public Vector3 collideWithWorld(Vector3 moverRadius, Vector3 pos, Vector3 vel, int recursionDepth) {
            if (recursionDepth > 5)
                return pos;

            CollisionPackage collisionPackage = collides(moverRadius, pos, vel);
            if (!collisionPackage.collision)
                return pos + vel;

            Vector3 destinationPoint = pos + vel;
            Vector3 newBasePoint = pos;

            if (collisionPackage.closestCollisionDistance >= 0.005) {
                Vector3 v = Vector3.Normalize(vel) * (collisionPackage.closestCollisionDistance - 0.005f);
                newBasePoint = collisionPackage.position + v;
                v.Normalize();
                collisionPackage.closestCollisionPoint -= 0.005f * v;
            }

            Vector3 slidingPlaneOrigin = collisionPackage.closestCollisionPoint;
            Vector3 slidingPlaneNormal = newBasePoint - collisionPackage.closestCollisionPoint;
            slidingPlaneNormal.Normalize();

            Vector3 newDestinationPoint = destinationPoint - signedDistance(slidingPlaneNormal, destinationPoint, -Vector3.Dot(slidingPlaneNormal, slidingPlaneOrigin)) * slidingPlaneNormal;

            Vector3 newVelocityVector = newDestinationPoint - collisionPackage.closestCollisionPoint;

            if (Math.Sqrt(Vector3.Dot(newVelocityVector, newVelocityVector)) < 0.005)
                return newBasePoint;

            return collideWithWorld(moverRadius, newBasePoint, newVelocityVector, recursionDepth + 1);
        }

        public Vector3 applyMovement(GameTime gameTime, PlayerIndex pi, Vector3 velocity) {
            if (!core.clip)
                return core.players[(int)pi].position + velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            return applyMovement(gameTime, core.players[(int)pi], velocity);
        }

        public Vector3 applyMovement(GameTime gameTime, Agent player, Vector3 velocity) {
            BoundingBox playerBoundingBox = player.getBoundingBox();

            //update the persistentVelocity
            Velocities pv = player.agentVelocities;
            if(pv.persistentVelocity.Y == 0)
                pv.persistentVelocity.Y = velocity.Y;
            velocity.Y = 0;
            pv.persistentVelocity.Y += -gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //fix it to ensure terminalVelcoity
            if (pv.persistentVelocity.Y <= player.terminalVelocity)
                pv.persistentVelocity.Y = player.terminalVelocity;

            Vector3 retMove = (velocity) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (Brush b in core.mapEngine.brushes)
                b.colliding = false;

            Vector3 offset = center(playerBoundingBox) - player.position;

            Vector3 playerRadius = size(playerBoundingBox) * 0.5f;
            Vector3 basePoint = toESpace(playerRadius, center(playerBoundingBox));
            Vector3 evelocity = toESpace(playerRadius, retMove);

            Vector3 pos = collideWithWorld(playerRadius, basePoint, evelocity, 0);

            Vector3 eSpacePersistent = toESpace(playerRadius, pv.persistentVelocity);
            Vector3 finalPos = collideWithWorld(playerRadius, pos, eSpacePersistent, 0);
            //Vector3 finalPos = collideWithWorld(playerRadius, pos, Vector3.Zero, 0);

            //check if we're on the floor
            //this happens if we were moving down and the velocity vector was modified
            Vector3 eSpaceActualMove = finalPos - pos;
            //if (Vector3.Distance(finalPos, pos) == 0 || Math.Abs(Vector3.Dot(Vector3.Normalize(pos-finalPos), Vector3.Up)) < 0.5) {
            if(eSpacePersistent.Y < 0 && eSpaceActualMove.Y - 0.005 > eSpacePersistent.Y)    {
                //Console.WriteLine(eSpacePersistent.Y + " " + eSpaceActualMove.Y);
                pv.persistentVelocity.Y = 0;
                //Console.WriteLine(floor++);
                finalPos = pos;
            }
            if (eSpacePersistent.Y > 0 && eSpaceActualMove.Y + 0.005 < eSpacePersistent.Y)
            {
                pv.persistentVelocity.Y = -0.005f;
            }

            //if (Math.Abs(finalPos.X - pos.X) > 0.005 || Math.Abs(finalPos.Z - pos.Z) > 0.005)finalPos = pos;

            return toWorldSpace(playerRadius, finalPos) - offset;
        }

        public class HitScan {
            public Vector3 collisionPoint;
            public Ray ray;
            public HitScan(Vector3 cp, Vector3 sp, Vector3 d)
            {
                collisionPoint = cp;
                ray = new Ray(sp, d);
            }

            public HitScan(Vector3 cp, Ray r)
            {
                collisionPoint = cp;
                ray = r;
            }

            public float Distance()
            {
                return Vector3.Distance(ray.Position, collisionPoint);
            }
        }

        public HitScan hitscan(Vector3 start, Vector3 dir, List<Brush> ignoreBrushes)
        {
            if (ignoreBrushes == null)
                ignoreBrushes = new List<Brush>();
            //first check against map geometry first
            dir = Vector3.Normalize(dir);
            Ray r = new Ray(start, dir);
            float closestDist = float.MaxValue;
            Vector3 closestPoint = Vector3.Zero;
            bool foundPoint = false;
            foreach (Brush brush in core.mapEngine.brushes)
            {
                if (ignoreBrushes.Contains(brush))
                    continue;
                foreach (Face f in brush.faces)
                {
                    Nullable<float> dist = r.Intersects(new Microsoft.Xna.Framework.Plane(f.plane.getNormal(), f.plane.getD()));
                    if (dist != null && (!foundPoint || dist.Value < closestDist))
                    {
                        //find that point
                        Vector3 p = r.Position + r.Direction * dist.Value;
                        if (MapEngine.pointOnFace(p, f))
                        {
                            closestDist = dist.Value;
                            closestPoint = p;
                            foundPoint = true;
                        }
                    }
                } // for face
            } // for brush

            //now check players
            if(foundPoint)
                return new HitScan(closestPoint, r);
            return null;
        }
    }
}
