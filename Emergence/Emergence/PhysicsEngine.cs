using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using Emergence.Map;
using Emergence.Pickup;

namespace Emergence {
    public interface ICollidable
    {
        BoundingBox getBoundingBox();
        List<CollisionGridCell> getCollisionCells();
        void setCollisionCells(List<CollisionGridCell> cells);
    }

    public class CollisionGridCell
    {
        public List<ICollidable> elements;

        public CollisionGridCell() {
            elements = new List<ICollidable>();
        }
    }

    public class Velocities {
        public Vector3 persistentVelocity = Vector3.Zero,
            momentumVelocity = Vector3.Zero;
    }

    public class PhysicsEngine {
        CoreEngine core;
        public CollisionGridCell[,,] grid;
        public float cellSize = 1;
        public Vector3 gridOffset = Vector3.Zero;
        public Vector3 boundingBoxPadding = new Vector3(10, 10, 10);
        //Velocities[] playerVelocities = { new Velocities(), new Velocities(), new Velocities(), new Velocities() };

        public float gravity = 17f;   //units per second

        public PhysicsEngine(CoreEngine c) {
            core = c;
            grid = new CollisionGridCell[0,0,0];
        }

        public void generateCollisionGrid(int cellSize) {
            //find the bounds of the entire map
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
                    max = -min;
            foreach (Brush brush in core.mapEngine.brushes) {
                BoundingBox b = brush.boundingBox;
                min.X = Math.Min(min.X, b.Min.X);
                min.Y = Math.Min(min.Y, b.Min.Y);
                min.Z = Math.Min(min.Z, b.Min.Z);
                max.X = Math.Max(max.X, b.Max.X);
                max.Y = Math.Max(max.Y, b.Max.Y);
                max.Z = Math.Max(max.Z, b.Max.Z);
            }
            min -= new Vector3(1, 1, 1) * cellSize;
            max += new Vector3(1, 1, 1) * cellSize;
            gridOffset = min;

            //now that we have the bounds, calculate the grid cell bounds
            grid = new CollisionGridCell[(int)Math.Max(1, Math.Ceiling((max.X - min.X) / cellSize)),
                                         (int)Math.Max(1, Math.Ceiling((max.Y - min.Y) / cellSize)),
                                         (int)Math.Max(1, Math.Ceiling((max.Z - min.Z) / cellSize))];
            for(int k = 0; k < grid.GetLength(2); ++k)
                for(int j = 0; j < grid.GetLength(1); ++j)
                    for (int i = 0; i < grid.GetLength(0); ++i)
                        grid[i, j, k] = new CollisionGridCell();
            this.cellSize = cellSize;
            foreach (ICollidable c in core.allCollidables())
                updateCollisionCellsFor(c);
        }

        public void updateCollisionCellsFor(ICollidable c) {
            //first clear the cells the c currently belongs to
            removeFromCollisionGrid(c);

            BoundingBox b = c.getBoundingBox();
            b.Min -= gridOffset;
            b.Max -= gridOffset;
            List<CollisionGridCell> collidablesCells = new List<CollisionGridCell>();
            //get the cell bounds for this
            for (int k = Math.Max(0, (int)Math.Floor(b.Min.Z / cellSize)); k <= Math.Min(grid.GetLength(2) - 1, (int)Math.Ceiling(b.Max.Z / cellSize)); ++k)
                for (int j = Math.Max(0, (int)Math.Floor(b.Min.Y / cellSize)); j <= Math.Min(grid.GetLength(1) - 1, (int)Math.Ceiling(b.Max.Y / cellSize)); ++j)
                    for (int i = Math.Max(0, (int)Math.Floor(b.Min.X / cellSize)); i <= Math.Min(grid.GetLength(0) - 1, (int)Math.Ceiling(b.Max.X / cellSize)); ++i) {
                        grid[i, j, k].elements.Add(c);
                        collidablesCells.Add(grid[i, j, k]);
                    }

            c.setCollisionCells(collidablesCells);
        }

        public void removeFromCollisionGrid(ICollidable c) {
            List<CollisionGridCell> cells = c.getCollisionCells();
            foreach (CollisionGridCell cell in cells)
                cell.elements.Remove(c);
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

        private IEnumerable<ICollidable> getCollidablesToCheck(ICollidable c, Vector3 vel) {
            BoundingBox bb = c.getBoundingBox();
            Vector3 minA = bb.Min - gridOffset, maxA = bb.Max - gridOffset;
            Vector3 minB = minA + vel, maxB = maxA + vel;
            Dictionary<ICollidable, bool> checkedCols = new Dictionary<ICollidable, bool>();
            for (int k = Math.Max(0, (int)Math.Floor(Math.Min(minA.Z, minB.Z) / cellSize)); k <= Math.Min(grid.GetLength(2) - 1, (int)Math.Ceiling(Math.Max(maxA.Z, maxB.Z) / cellSize)); ++k)
                for (int j = Math.Max(0, (int)Math.Floor(Math.Min(minA.Y, minB.Y) / cellSize)); j <= Math.Min(grid.GetLength(1) - 1, (int)Math.Ceiling(Math.Max(maxA.Y, maxB.Y) / cellSize)); ++j)
                    for (int i = Math.Max(0, (int)Math.Floor(Math.Min(minA.X, minB.X) / cellSize)); i <= Math.Min(grid.GetLength(0) - 1, (int)Math.Ceiling(Math.Max(maxA.X, maxB.X) / cellSize)); ++i)
                        foreach(ICollidable col in grid[i,j,k].elements)
                            if (!checkedCols.ContainsKey(col)) {
                                checkedCols.Add(col, true);
                                yield return col;
                            }
        }

        public bool collidesWithPickup(Vector3 moverRadius, Vector3 basePoint, Vector3 velocity, PickUp collidable) {
            /* we cast a ray from the center of collider to the center of
             * collidable. if the collision time is closer (or sufficiently close)
             * for collidable then we're colliding
             */
            BoundingBox otherbb = ((ICollidable)collidable).getBoundingBox();
            Vector3 otherRadius = (otherbb.Max - otherbb.Min) * 0.5f,
                    otherPos = otherbb.Min + otherRadius;

            //now convert everything otherESpace so that we can cast using BoundingSpheres
            Vector3 basePointInOtherESpace = toESpace(otherRadius, toWorldSpace(moverRadius, basePoint)),
                    otherPointInOtherESPace = toESpace(otherRadius, otherPos);
            Ray r = new Ray(basePointInOtherESpace, Vector3.Normalize(otherPointInOtherESPace - basePointInOtherESpace));
            Nullable<float> dist = r.Intersects(new BoundingSphere(otherPointInOtherESPace, 1));
            if (dist != null) { //i don't even know what dist == null means in this context
                //find the collisionPoint in otherESpace
                Vector3 colPoint = r.Position + dist.Value * r.Direction;
                //convert it back to normal eSpace (of basePoint)
                colPoint = toESpace(moverRadius, toWorldSpace(otherRadius, colPoint));
                float colDist = Vector3.Distance(colPoint, basePoint);
                //if the distance is < 1 + epsilon (the radius of the collider in eSpace) then we've collided
                if (colDist < 1 + 0.5) {
                    float t = colDist / (float)Math.Sqrt(Vector3.Dot(velocity, velocity));
                    return true;
                }
            }
            return false;
        }

        //picks up and applies any pickups the agent will take in the next move
        public void applySimpleMovement(GameTime gameTime, Agent a, Vector3 velocity) {
            BoundingBox agentBoundingBox = a.getBoundingBox();
            Vector3 agentRadius = size(agentBoundingBox) * 0.5f;
            Vector3 agentPoint = agentBoundingBox.Min + agentRadius;
            velocity = velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 agentVelocity = toESpace(agentRadius, velocity);
            List<PickUp> pickedUp = new List<PickUp>();

            foreach(ICollidable collidable in getCollidablesToCheck(a, velocity))
                if (collidable is PickUp) {
                    PickUp p = (PickUp)collidable;
                    if (collidesWithPickup(agentRadius, agentPoint, agentVelocity, p)) {
                        Console.WriteLine("picked up");
                        pickedUp.Add(p);
                    }
                }

            foreach (PickUp p in pickedUp) {
                p.pickupGen.removePickUp();
                p.affect(a);
            }

            a.position += velocity;
        }

        public CollisionPackage collides(Vector3 moverRadius, Vector3 basePoint, Vector3 velocity, ICollidable collider) {
            float closestCollisionTime = 1;
            Vector3 closestCollisionPoint = Vector3.Zero;

            //make colliderBoundingBox a bounding volume over the movement
            BoundingBox colliderBoundingBox = collider.getBoundingBox();
            Vector3 worldVel = toWorldSpace(moverRadius, velocity);
            if (worldVel.X < 0) colliderBoundingBox.Min.X += worldVel.X;
            else if (worldVel.X > 0) colliderBoundingBox.Max.X += worldVel.X;
            if (worldVel.Y < 0) colliderBoundingBox.Min.Y += worldVel.Y;
            else if (worldVel.Y > 0) colliderBoundingBox.Max.Y += worldVel.Y;
            if (worldVel.Z < 0) colliderBoundingBox.Min.Z += worldVel.Z;
            else if (worldVel.Z > 0) colliderBoundingBox.Max.Z += worldVel.Z;

            List<PickUp> pickedUp = new List<PickUp>();

            List<Vector3> eSpaceVerts = new List<Vector3>();
            bool convertedVerts = false;
            foreach(ICollidable collidable in getCollidablesToCheck(collider, toWorldSpace(moverRadius, velocity)))    {
                if (collidable is Agent)    continue;
                BoundingBox collidableBoundingBox = collidable.getBoundingBox();
                collidableBoundingBox.Min -= boundingBoxPadding;
                collidableBoundingBox.Max += boundingBoxPadding;
                if (!collidableBoundingBox.Intersects(colliderBoundingBox) || collidable == collider) continue;
                if(collidable is Brush) {
                    Brush brush = (Brush)collidable;
                    bool faceCollide = false;
                    foreach (Face face in brush.faces) {
                        if (faceCollide)
                            break;
                        convertedVerts = false;

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
                            t1 = (1 - signedDistance(N, basePoint, D)) / Vector3.Dot(N, velocity);
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
                            eSpaceVerts.Clear();
                            foreach (Vertex vert in face.vertices)
                                eSpaceVerts.Add(toESpace(moverRadius, vert.position));
                            convertedVerts = true;
                            if (MapEngine.pointOnFace(planeIntersectionPoint, eSpaceVerts, N)) {
                                //float intersectionDistance = t0 * (float)Math.Sqrt(Vector3.Dot(velocity, velocity));
                                if (face.DiffuseColor == new Vector3(1, 1, 1))
                                    face.DiffuseColor = new Vector3(1, 0, 0);
                                if (t0 < closestCollisionTime) {
                                    closestCollisionTime = t0;
                                    closestCollisionPoint = planeIntersectionPoint;
                                    faceCollide = true;
                                    continue;
                                }
                            }
                        }
                        if (!convertedVerts) {
                            eSpaceVerts.Clear();
                            foreach (Vertex vert in face.vertices)
                                eSpaceVerts.Add(toESpace(moverRadius, vert.position));
                            convertedVerts = true;
                        }

                        //do sweep tests
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
                                    root = r2;
                                /* intersectionPoint = p;
                                 * intersectionDistance = x1 * (float)Math.Sqrt(Vector3.Dot( velocity, velocity ));
                                 */
                                if (root != -1) {
                                    closestCollisionTime = root;
                                    closestCollisionPoint = p;
                                    faceCollide = true;
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
                                        faceCollide = true;
                                    }
                                }
                            }
                        }
                        
                    }
                } // if brush
                else if(collider is Agent && collidable is PickUp) {  //otherwise we need to boundingEllipsoid collisions
                    PickUp p = (PickUp)collidable;
                    if (collidesWithPickup(moverRadius, basePoint, velocity, p))
                        pickedUp.Add(p);
                } // if pickup
            }

            foreach (PickUp p in pickedUp) {
                p.pickupGen.removePickUp();
                p.affect((Agent)collider);
            }
            return new CollisionPackage(basePoint, velocity, closestCollisionTime < 1, closestCollisionPoint, closestCollisionTime * (float)Math.Sqrt(Vector3.Dot(velocity, velocity)));
        }

        public Vector3 collideWithWorld(Vector3 moverRadius, Vector3 pos, Vector3 vel, int recursionDepth, ICollidable collidable) {
            if (recursionDepth > 5)
                return pos;

            CollisionPackage collisionPackage = collides(moverRadius, pos, vel, collidable);
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

            return collideWithWorld(moverRadius, newBasePoint, newVelocityVector, recursionDepth + 1, collidable);
        }

        public void applyMovement(GameTime gameTime, PlayerIndex pi, Vector3 velocity) {
            if (!core.clip)
                core.getPlayerForIndex(pi).position = core.getPlayerForIndex(pi).position + velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            else
                applyMovement(gameTime, core.getPlayerForIndex(pi), velocity);
        }

        public void applyMovement(GameTime gameTime, Agent player, Vector3 velocity) {
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

            Vector3 pos = collideWithWorld(playerRadius, basePoint, evelocity, 0, player);

            Vector3 eSpacePersistent = toESpace(playerRadius, pv.persistentVelocity);
            Vector3 finalPos = collideWithWorld(playerRadius, pos, eSpacePersistent, 0, player);
            //Vector3 finalPos = collideWithWorld(playerRadius, pos, Vector3.Zero, 0);

            //check if we're on the floor
            //this happens if we were moving down and the velocity vector was modified
            Vector3 eSpaceActualMove = finalPos - pos;
            if(eSpacePersistent.Y < 0 && eSpaceActualMove.Y - 0.005 > eSpacePersistent.Y)    {
                pv.persistentVelocity.Y = 0;
                finalPos = pos;
            }
            if (eSpacePersistent.Y > 0 && eSpaceActualMove.Y + 0.005 < eSpacePersistent.Y)
            {
                pv.persistentVelocity.Y = -0.005f;
            }

            player.position = toWorldSpace(playerRadius, finalPos) - offset;
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
