using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Emergence.Map {
    public class Face {
        public List<Vertex> vertices;
        public Plane plane;
        public Vector3 DiffuseColor = new Vector3(1, 1, 1);

        public Face(List<Vertex> v, Plane p) {
            vertices = v;
            //Console.WriteLine("Face has " + vertices.Count + " vertices");
            plane = p;

            //fix the plane's first, second and third points
            Vector3 x = Vector3.Normalize(plane.meshThird - plane.meshSecond),
                    y = Vector3.Normalize(plane.meshFirst - plane.meshSecond);

            //find the minCorner as a (x,y)
            Vector2 minCorner = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxCorner = new Vector2(-float.MaxValue, -float.MaxValue);
            foreach (Vertex c in vertices) {
                Vector2 cc = new Vector2(Vector3.Dot(c.position - plane.meshSecond, x), Vector3.Dot(c.position - plane.meshSecond, y));
                minCorner.X = Math.Min(cc.X, minCorner.X);
                minCorner.Y = Math.Min(cc.Y, minCorner.Y);
                maxCorner.X = Math.Max(cc.X, maxCorner.X);
                maxCorner.Y = Math.Max(cc.Y, maxCorner.Y);
            }

            plane.meshSecond = minCorner.X * x + minCorner.Y * y + plane.meshSecond;
            plane.meshThird = (maxCorner.X - minCorner.X) * x + plane.meshSecond;
            plane.meshFirst = (maxCorner.Y - minCorner.Y) * y + plane.meshSecond;

            windVertices();
            computeTexCoords();
        }

        private void windVertices() {
            Vector3 n = plane.getNormal();

            //find the center of the polygon by taking the average of the vertices
            Vector3 cent = getCenter();

            //selection sort these guys using the cosVal function
            for (int i = 0; i < vertices.Count-1; i++) {
                int minVert = i+1;
                double minCosVal = cosVal(cent, n, vertices[i].position, vertices[i+1].position);
                for (int j = i + 2; j < vertices.Count; j++) {
                    double cv = cosVal(cent, n, vertices[i].position, vertices[j].position);
                    if (cv < minCosVal) {
                        minVert = j;
                        minCosVal = cv;
                    }
                }
                Vertex v = vertices[minVert];
                vertices.RemoveAt(minVert);
                vertices.Insert(i + 1, v);
            }

            //calculate the normal of this face and if it doesn't match up with the plane, reverse the winding
            //this inequailty and the inequality in the cosVal function seem linked somehow
            if (Vector3.Dot(Vector3.Cross(vertices[0].position - cent, vertices[1].position - cent), n) > 0)
                vertices.Reverse();
        }

        private Vector2 calcTexCoords(Vector3 vec) {
            Vector3 uAxis = plane.third - plane.second,
                    vAxis = plane.first - plane.second;
            uAxis.Normalize(); vAxis.Normalize();
            Vector3 diff = vec - plane.second;
            return new Vector2((float)((Vector3.Dot(uAxis, diff) - plane.hOffset) / (plane.texture.Width * plane.hScale)),
                               (float)((Vector3.Dot(vAxis, diff) - plane.vOffset) / (plane.texture.Height * plane.vScale)));
        }

        //computes and stores the tex coordinates for all the vertices
        private void computeTexCoords() {
            //U is the X axis of the texture
            Vector3 uAxis = plane.third - plane.second,
                    vAxis = plane.first - plane.second;
            uAxis.Normalize(); vAxis.Normalize();

            foreach (Vertex v in vertices) {
                Vector3 diff = v.position - plane.second;
                v.texCoord = new Vector2((float)((Vector3.Dot(uAxis, diff) - plane.hOffset) / (plane.texture.Width*plane.hScale)),
                                        (float)((Vector3.Dot(vAxis, diff)-plane.vOffset)/(plane.texture.Height*plane.vScale)));
                //Console.WriteLine(v.texCoord);
            }
        }

        public Vector3 getCenter() {
            Vector3 cent = Vector3.Zero;
            foreach(Vertex v in vertices)
                cent = cent + v.position;
            cent = Vector3.Multiply(cent, 1f/vertices.Count);
            return cent;
        }

        /* This function takes in two vertices on the polygon that makes up this face (v1 and v2).
         * It also takes the center and normal of the polygon.
         * Then it returns a number in [0,2) which represents the rotation v2 is from v1 (relative to the center).
         * 0 represents a zero angle, 1 represents 180 degrees and 2 represents 360 degrees
         */
        public double cosVal(Vector3 cent, Vector3 n, Vector3 v1, Vector3 v2) {
            Vector3 vv1 = v1 - cent, vv2 = v2 - cent;
            vv1.Normalize(); vv2.Normalize();
            //this gives us a symmetric angle between v1 and v2 in [-1,1] where -1 = 0 degrees and 1 = 180 degrees
            double cv = -Vector3.Dot(vv1, vv2);
            //transform this to [0,1] space
            cv = (cv + 1) / 2;
            //transform this in to [0,2] space
            if (cv > 0) {
                //now take the cross product value of the two vectors
                Vector3 cross = Vector3.Cross(vv1, vv2);
                //if the cross product is not pointing in same direction as the normal, then add 1 to cv
                //this introduces an asymmetry
                if (Vector3.Dot(cross, n) < 0)
                    cv += 1;
            }
            return cv;
        }

        public VertexPositionNormalTexture[] getPoints() {
            VertexPositionNormalTexture[] points = new VertexPositionNormalTexture[vertices.Count];

            /*VertexPositionNormalTexture cent = new VertexPositionNormalTexture();
            cent.Normal = plane.getNormal();
            cent.Position = getCenter();
            cent.TextureCoordinate = calcTexCoords(getCenter());
            points[0] = cent;*/

            for (int i = 0; i < vertices.Count; i++) {
                VertexPositionNormalTexture p = new VertexPositionNormalTexture();
                p.Position = vertices[i].position;
                p.Normal = plane.getNormal();
                p.TextureCoordinate = vertices[i].texCoord;
                points[i] = p;
            }
            return points;
        }
    }
}
