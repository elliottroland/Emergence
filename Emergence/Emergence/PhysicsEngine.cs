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

        class LineSegment {
            public Vector3 a, b;
            public LineSegment(Vector3 a, Vector3 b) {
                this.a = a;
                this.b = b;
            }
        }



        public Vector3 getPushOut(BoundingBox mover, Brush brush, Vector3 move) {
            return Vector3.Zero;
        }

        public Vector3 applyCollision(GameTime gameTime, PlayerIndex pi, Vector3 velocity) {
            Player player = core.players[(int)pi];
            BoundingBox playerBoundingBox = player.getBoundingBox();

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

            return retMove;
        }
    }
}
