using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Emergence.Map {
    public class Plane {
        public Vector3 first, second, third, meshFirst, meshSecond, meshThird;
        public Texture2D texture;
        public double hOffset, vOffset, textureRotation, hScale, vScale;

        public Microsoft.Xna.Framework.Plane plane;

        public Plane(Vector3 first, Vector3 second, Vector3 third, Texture2D texture, double hO, double vO, double tR, double hS, double vS) {
            //a plane is defined by three points. we want the second of the points to be the corner so that first -> second -> third goes along
            //two edges of the plane
            this.first = MapEngine.toWorldSpace(first);
            this.second = MapEngine.toWorldSpace(second);
            this.third = MapEngine.toWorldSpace(third);

            meshFirst = this.first;
            meshSecond = this.second;
            meshThird = this.third;

            this.texture = texture;

            plane = new Microsoft.Xna.Framework.Plane(this.first, this.second, this.third);
            //this accounts for a different winding from XNA
            plane.Normal = -plane.Normal;

            fixMeshOrder();

            //if a,b,c are our points, then b is the corner if and only if dot(a-b, c-b) = 0
            //but because we don't have exact values, we want to take the one which is closest to 0
            /*double dot = Math.Abs(Vector3.Dot(this.first - this.second, this.third - this.second));
            int minVec = 2;
            if (Math.Abs(Vector3.Dot(this.second - this.first, this.third - this.first)) < dot) {
                dot = Math.Abs(Vector3.Dot(this.second - this.first, this.third - this.first));
                minVec = 1;
            }
            if (Math.Abs(Vector3.Dot(this.second - this.third, this.first - this.third)) < dot) {
                dot = Math.Abs(Vector3.Dot(this.second - this.third, this.first - this.third));
                minVec = 3;
            }

            //now we do the appropriate swap
            if (minVec == 1) {
                Vector3 temp = this.second;
                this.second = this.first;
                this.first = temp;
            }
            else if (minVec == 3) {
                Vector3 temp = this.second;
                this.second = this.third;
                this.third = temp;
            }
            else
                Console.WriteLine("Second was the corner");*/

            hOffset = hO;
            vOffset = vO;
            textureRotation = tR;
            hScale = hS;
            vScale = vS;
        }

        public void fixMeshOrder() {
            //swap the accordingly
            float dot = Math.Abs(Vector3.Dot(meshFirst - meshSecond, meshThird - meshSecond));
            if (Math.Abs(Vector3.Dot(meshFirst - meshThird, meshSecond - meshThird)) < dot) {
                Vector3 temp = meshSecond;
                meshSecond = meshThird;
                meshThird = temp;
                dot = Math.Abs(Vector3.Dot(meshFirst - meshThird, meshSecond - meshThird));
            }
            if (Math.Abs(Vector3.Dot(meshThird - meshFirst, meshSecond - meshFirst)) < dot) {
                Vector3 temp = meshSecond;
                meshSecond = meshFirst;
                meshFirst = temp;
                dot = Math.Abs(Vector3.Dot(meshThird - meshFirst, meshSecond - meshFirst));
            }
        }

        public Vector3 getNormal()  {
            /*Vector3 v = Vector3.Cross(first-second, third-second);
            v.Normalize();
            return v;*/
            return plane.Normal;
        }

        //planes can be written as ax+by+cz+d=0, this returns the d value
        public float getD() {
            //return -Vector3.Dot(second, getNormal());
            return -plane.D;
        }

        public Vector3 getCenter() {
            return Vector3.Multiply(second - first, 0.5f) + Vector3.Multiply(third, 0.5f);
        }

        public static int pointsInCommon(Vector3 a, Vector3 b) {
            int pic = 0;
            if (a.X == b.X)
                pic++;
            if (a.Y == b.Y)
                pic++;
            if (a.Z == b.Z)
                pic++;
            return pic;
        }
    }
}
