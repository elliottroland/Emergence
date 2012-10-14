using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Emergence.Map {
    public class Brush : ICollidable {
        public List<Plane> planes;
        public List<Face> faces;
        public int verts = 0;
        public BoundingBox boundingBox;
        public bool colliding = false;
        protected List<CollisionGridCell> collisionCells;

        public Brush() {
            planes = new List<Plane>();
            collisionCells = new List<CollisionGridCell>();
        }

        public void generateFaces() {
            boundingBox = new BoundingBox(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue));
            //this will be the list of faces which we're generating
            faces = new List<Face>();
            this.verts = 0;

            //first calculate all the vertices
            //verts is an array of lists, each list containing the vertices of a given face which lies on a plane
            List<Vertex>[] verts = new List<Vertex>[planes.Count];

            //initialise all the empty lists
            for (int i = 0; i < planes.Count; i++)
                verts[i] = new List<Vertex>();
            
            //fill the lists by calculating all the vertices
            //we do this by finding all the places that three planes intersect
            //natrally, that vertex belongs to each of those faces
            for (int i = 0; i < planes.Count; i++)
                for (int j = i+1; j < planes.Count; j++ ) {
                    if (i != j)
                        for (int k = j+1; k < planes.Count; k++ ) {
                            if (j != k && i != k) {
                                //create a vertex
                                Vertex v = getIntersection(planes[i], planes[j], planes[k]);
                                if (v != null && contains(v)) {
                                    verts[i].Add(v);
                                    verts[j].Add(new Vertex(v));    //we don't want to add the SAME vertex, since the vertices will have different properties when they're in different faces (like texCoords)
                                    verts[k].Add(new Vertex(v));
                                    this.verts++;                   //keep track of the number of unique vertex positions we have
                                    boundingBox.Min.X = (float)Math.Min(v.position.X, boundingBox.Min.X);
                                    boundingBox.Min.Y = (float)Math.Min(v.position.Y, boundingBox.Min.Y);
                                    boundingBox.Min.Z = (float)Math.Min(v.position.Z, boundingBox.Min.Z);
                                    boundingBox.Max.X = (float)Math.Max(v.position.X, boundingBox.Max.X);
                                    boundingBox.Max.Y = (float)Math.Max(v.position.Y, boundingBox.Max.Y);
                                    boundingBox.Max.Z = (float)Math.Max(v.position.Z, boundingBox.Max.Z);
                                }
                            }
                        }
                }

            //now for each plane, create the face
            for (int i = 0; i < planes.Count; i++)
                if(verts[i].Count >= 3)
                    faces.Add(new Face(verts[i], planes[i]));
        }

        public bool contains(Vertex v) {
            //Console.WriteLine("\n");
            foreach (Plane p in planes) {
                //Console.WriteLine(v.position + " " + p.getNormal() + " " +  p.getD());
                //Console.WriteLine(Vector3.Dot(v.position, p.getNormal()) + p.getD());
                if(Vector3.Dot(v.position, p.getNormal()) + p.getD() > 0.04)
                    return false;
            }
            return true;
        }

        public Vertex getIntersection(Plane p1, Plane p2, Plane p3) {
            return getIntersection(p1.getNormal(), p2.getNormal(), p3.getNormal(), (float)(p1.getD()), (float)(p2.getD()), (float)(p3.getD()));
        }

        //might need to convert these floats to doubles for accuracy sake
        public Vertex getIntersection(Vector3 n1, Vector3 n2, Vector3 n3, float d1, float d2, float d3) {
            float denom = Vector3.Dot(n1, Vector3.Cross(n2, n3));
            if (denom == 0)
                return null;
            //p = -d1 * ( n2.Cross ( n3 ) ) – d2 * ( n3.Cross ( n1 ) ) – d3 * ( n1.Cross ( n2 ) ) / denom;
            Vertex v = new Vertex(Vector3.Multiply(Vector3.Multiply(Vector3.Cross(n2, n3), -d1) + Vector3.Multiply(Vector3.Cross(n3,n1), -d2) + Vector3.Multiply(Vector3.Cross(n1, n2), -d3), 1/denom));
            return v;
        }

        List<CollisionGridCell> ICollidable.getCollisionCells() {
            return collisionCells;
        }

        void ICollidable.setCollisionCells(List<CollisionGridCell> cells) {
            collisionCells = cells;
        }

        BoundingBox ICollidable.getBoundingBox() {
            return boundingBox;
        }
    }
}
