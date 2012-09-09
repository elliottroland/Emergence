using System;
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

        public Vector3 getPushOut(BoundingBox mover, BoundingBox stationary, Vector3 move) {
            BoundingBox afterMove = new BoundingBox(mover.Min + move, mover.Max + move);
            BoundingBox merge = BoundingBox.CreateMerged(afterMove, stationary);
            Vector3 intersectionSize = size(mover) + size(stationary) - size(merge);

            if (Math.Min(Math.Min(intersectionSize.X, intersectionSize.Y), intersectionSize.Z) <= 0)
                return Vector3.Zero;

            double val = Double.MaxValue;
            Vector3 pushout = Vector3.Zero;
            Vector3 pushDir = center(mover) - center(stationary);
            pushDir = new Vector3(Math.Sign(pushDir.X), Math.Sign(pushDir.Y), Math.Sign(pushDir.Z));
            if (intersectionSize.X > 0) {
                if (intersectionSize.X < val) {
                    val = intersectionSize.X;
                    //pushout = new Vector3(-Math.Sign(move.X) * Math.Min(Math.Abs(move.X), intersectionSize.X), 0, 0);
                    pushout = new Vector3(pushDir.X * intersectionSize.X, 0, 0);
                }
            }
            if (intersectionSize.Z > 0) {
                if (intersectionSize.Z < val) {
                    val = intersectionSize.Z;
                    //pushout = new Vector3(0, 0, -Math.Sign(move.Z) * Math.Min(Math.Abs(move.Z), intersectionSize.Z));
                    pushout = new Vector3(0, 0, pushDir.Z * intersectionSize.Z);
                }
            }
            if (intersectionSize.Y > 0) {
                if (intersectionSize.Y < val) {
                    val = intersectionSize.Y;
                    //pushout = new Vector3(0, -Math.Sign(move.Y) * Math.Max(Math.Abs(move.Y), intersectionSize.Y), 0);
                    pushout = new Vector3(0, pushDir.Y * intersectionSize.Y, 0);
                }
            }
            return pushout;
        }
        /*
        //we use these to decide which plane to do pushing out for on a brush
        //such a plane must be 
        class planarCheck : IComparable {


            public int CompareTo(object other) {
                planarCheck o = other as planarCheck;
                return 0;
            }
        }

        public Vector3 getPushOut(BoundingBox mover, BoundingBox stationary, Vector3 move) {
            BoundingBox afterMove = new BoundingBox(mover.Min + move, mover.Max + move);
            BoundingBox merge = BoundingBox.CreateMerged(afterMove, stationary);
            return Vector3.Zero;
        }*/

        public bool onFloor(GameTime gameTime, PlayerIndex pi) {
            BoundingBox playerBoundingBox = core.players[(int)pi].getBoundingBox();
            Vector3 gravity = new Vector3(0, -(float)gameTime.ElapsedGameTime.TotalSeconds * this.gravity, 0);
            //Vector3 gravity = getGravity(gameTime, pi) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (Brush b in core.mapEngine.brushes) {
                if (playerBoundingBox.Intersects(b.boundingBox)) {
                    Vector3 po = getPushOut(core.players[(int)pi].getBoundingBox(), b.boundingBox, gravity);
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
            Velocities pv = playerVelocities[(int)pi];
            pv.persistentVelocity.Y = Math.Max(pv.persistentVelocity.Y - core.physicsEngine.gravity, player.terminalVelocity);
            bool playerOnFloor = onFloor(gameTime, pi);
            if (playerOnFloor) {
                pv.persistentVelocity.Y = velocity.Y;
                pv.momentumVelocity = Vector3.Zero;
            }
            velocity.Y = 0; //we've extracted this effect in the above if-statement

            Vector3 retMove = (velocity + pv.persistentVelocity) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 originalRetMove = new Vector3(retMove.X, retMove.Y, retMove.Z);
            foreach (Brush b in core.mapEngine.brushes) {
                if (playerBoundingBox.Intersects(b.boundingBox)) {
                    Vector3 po = getPushOut(playerBoundingBox, b.boundingBox, retMove);
                    if (po != Vector3.Zero) {
                        retMove = retMove + po;
                    }
                }
            }

            if (retMove.Y != originalRetMove.Y)
                pv.persistentVelocity.Y = 0;

            return retMove;
        }
    }
}
