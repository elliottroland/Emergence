using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using Emergence.Map;

namespace Emergence.AI {
    public class MeshNode {
        public Vector3 position;
        public Face face;
        public List<MeshNode> neighbours;

        public MeshNode(Vector3 pos, Face f) {
            position = pos;
            face = f;
            neighbours = new List<MeshNode>();
        }
    }
    public class AIEngine {
        public static float floorAngle = 45,    //degrees from the up vector that is valid floor
                          nodeLift = 0.5f,   //distance above the planes to spawn the mesh nodes
                          nodeHeight = 60f, //the distance above the nodeLift that the node 'sees' from
                          nodeRenderHeight = 10f,
                          nodeRadius = 100;    //radius of nodes, this is how far they're laid away from walls (kinda) and other nodes
        CoreEngine core;
        public List<MeshNode> mesh;
        public List<AIAgent> agents;
        public Random random;           //a random for AI agents to use

        public AIEngine(CoreEngine core) {
            this.core = core;
            mesh = new List<MeshNode>();
            agents = new List<AIAgent>();
            random = new Random();
        }

        public void generateAIMesh()    {
            mesh = new List<MeshNode>();
            //generate priliminary list of meshNodes from faces that point vaguely up
            float floorDot = (float)Math.Cos(MathHelper.ToRadians(floorAngle));

            foreach(Brush brush in core.mapEngine.brushes)   {
                foreach (Face f in brush.faces) {
                    //if plane is valid "floor"
                    if (Vector3.Dot(f.plane.getNormal(), Vector3.Up) > floorDot) {
                        //we move along the "plane" that the face sits on
                        //and then for each valid point on the face, we put a node
                        Vector3 x = f.plane.meshThird - f.plane.meshSecond,
                                y = f.plane.meshFirst - f.plane.meshSecond;
                        float width = (float)Math.Sqrt(Vector3.Dot(x, x)),
                              height = (float)Math.Sqrt(Vector3.Dot(y, y));
                        x.Normalize();
                        y.Normalize();

                        //we need the face normal for node lift
                        Vector3 N = Vector3.Normalize(f.plane.getNormal());

                        for(float i = nodeRadius; i < height; i += nodeRadius)
                            for (float j = nodeRadius; j < width; j += nodeRadius) {
                                //get the point represented by (j,i)
                                //Vector3 p = f.plane.meshSecond + j * x + i * y + N * nodeLift;
                                Vector3 p = f.plane.meshSecond + j * x + i * y + new Vector3(0, nodeLift, 0);

                                //now find if this point is on the face
                                if (MapEngine.pointOnFace(p, f) && !core.mapEngine.pointInBrush(p))
                                {
                                    mesh.Add(new MeshNode(p + new Vector3(0,nodeHeight-nodeLift,0), f));
                                }
                            }
                    } // if "floor"
                } // for face
            } // for brush

            //connect vibes
            foreach(MeshNode m1 in mesh)   {
                foreach (MeshNode m2 in mesh)
                {
                    if (m1 == m2 || m1.neighbours.Contains(m2))
                        continue;
                    //now check if they're ok to connect
                    if (Vector3.Distance(m1.position, m2.position) > nodeRadius * 2.2)
                        continue;
                    //now check if they are line-of-sight
                    //DO THIS ----------------------------------------------------------------
                    PhysicsEngine.HitScan hs = core.physicsEngine.hitscan(m1.position, m2.position - m1.position, null);
                    if (hs != null && hs.Distance() < Vector3.Distance(m1.position, m2.position) - 0.005)
                        continue;

                    //now check if they're connecteable using the y-project intersection test
                    double ad = m1.face.plane.getD(),
                          bd = m2.face.plane.getD();
                    Vector3 a = m1.face.plane.getNormal(),
                            b = m2.face.plane.getNormal(),
                            ap = m1.position,
                            bp = m2.position;
                    if (Vector3.Distance(a, b) > 0.005)
                    {
                        double denom = (bp.X - ap.X) * (a.X * b.Y - b.X * a.Y) + (bp.Z - ap.Z) * (a.Z * b.Y - b.Z * a.Y);
                        if (denom == 0)
                            continue;
                        double t = (a.Y * (bd + b.X * ap.X + b.Z * ap.Z) - b.Y * (ad + a.X * ap.X + a.Z * ap.Z)) / denom;
                        if (0 <= t && t <= 1)
                        {
                            m1.neighbours.Add(m2);
                            m2.neighbours.Add(m1);
                        }
                    }
                    else if(Math.Abs(ad-bd) < 0.5)
                    {
                        m1.neighbours.Add(m2);
                        m2.neighbours.Add(m1);
                    }
                }
            }
        }

        public void Update(GameTime gameTime) {
            foreach (AIAgent a in agents)
                a.Update(gameTime);
        }
    }
}
