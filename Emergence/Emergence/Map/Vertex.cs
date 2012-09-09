using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Emergence.Map {
    public class Vertex {
        public Vector3 position;
        public Vector2 texCoord = Vector2.Zero;

        public Vertex(Vector3 pos) {
            position = pos;
        }

        public Vertex(Vertex other) {
            position = new Vector3(other.position.X, other.position.Y, other.position.Z);
        }
    }
}
