using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.RegularExpressions;

using Emergence.Render;

namespace Emergence.Map {
    public class Light
    {
        public Vector3 position;
        public Vector4 colour;
        public float radius;

        public Light(Vector3 p, Vector4 c, float r)
        {
            position = p;
            colour = c;
            radius = r;
        }
    }

    public class MapEngine {
        public Dictionary<String, Texture2D> textures;
        public List<Brush> brushes;
        public List<Light> lights;
        CoreEngine core;

        public MapEngine(CoreEngine c, String filename, Dictionary<String, Texture2D> textures) {
            core = c;
            this.textures = textures;
            brushes = new List<Brush>();
            lights = new List<Light>();
            List<Vector3> playerPoss = new List<Vector3>();

            //load the map
            using (System.IO.StreamReader sr = System.IO.File.OpenText(filename)) {
                string s = "";
                int level = 0;
                Brush curBrush = null;
                Dictionary<String, String> keyVals = new Dictionary<string, string>();
                while ((s = sr.ReadLine()) != null) {
                    if (s.StartsWith("{")) {
                        level++;
                        if (level == 2)
                            curBrush = new Brush();
                    }
                    else if (s.StartsWith("}")) {
                        level--;
                        if (level == 1 && curBrush != null && keyVals["classname"] == "worldspawn") {
                            curBrush.generateFaces();
                            brushes.Add(curBrush);
                            //Console.WriteLine("Added brush with " + curBrush.verts + " vertices and " + curBrush.faces.Count + " faces");
                        }
                        if (level <= 1)
                            curBrush = null;
                        if (level == 0) {
                            if (keyVals["classname"] == "info_player_start" && keyVals.ContainsKey("origin")) {
                                string [] d = Regex.Split(keyVals["origin"], @"\s+");
                                //core.renderEngine.cameraPosition = toWorldSpace(new Vector3(float.Parse(d[0]), float.Parse(d[1]), float.Parse(d[2])+56));
                                //core.players[0].position = toWorldSpace(new Vector3(float.Parse(d[0]), float.Parse(d[1]), float.Parse(d[2])));
                                playerPoss.Add(toWorldSpace(new Vector3(float.Parse(d[0]), float.Parse(d[1]), float.Parse(d[2]))));
                                //Console.WriteLine("set player's position to " + core.players[0].position);
                            }
                            else if (keyVals["classname"] == "light" && keyVals.ContainsKey("light") && keyVals.ContainsKey("origin"))
                            {
                                string[] d = Regex.Split(keyVals["origin"], @"\s+");
                                //figure out the colour
                                string[] col = Regex.Split(keyVals["colour"], @"\s+");
                                Vector4 colour = new Vector4(1, 1, 1, 1);
                                if (col.Length > 0)
                                    colour.X = float.Parse(col[0]);
                                if (col.Length > 1)
                                    colour.Y = float.Parse(col[1]);
                                if (col.Length > 2)
                                    colour.Z = float.Parse(col[2]);
                                if (col.Length > 3)
                                    colour.W = float.Parse(col[3]);
                                lights.Add(new Light(toWorldSpace(new Vector3(float.Parse(d[0]), float.Parse(d[1]), float.Parse(d[2]))),
                                                    colour,
                                                    float.Parse(keyVals["light"])));
                            }
                            keyVals = new Dictionary<string, string>();
                        }
                    }
                    else if (level == 1 && s.StartsWith("\"")) {
                        string key = s.Substring(1);
                        key = key.Substring(0, key.IndexOf("\""));
                        string val = s.Substring(key.Length + 4);
                        val = val.Substring(0, val.Length-1);
                        keyVals[key] = val;
                    }
                    else if (level == 2 && keyVals["classname"] == "worldspawn") {
                        string[] d = Regex.Split(s, @"\s+");
                        Emergence.Map.Plane p = new Emergence.Map.Plane(new Vector3(float.Parse(d[1]), float.Parse(d[2]), float.Parse(d[3])),
                                                                                     new Vector3(float.Parse(d[6]), float.Parse(d[7]), float.Parse(d[8])),
                                                                                     new Vector3(float.Parse(d[11]), float.Parse(d[12]), float.Parse(d[13])),
                                                                                     textures[d[15]], //the texture name
                                                                                     double.Parse(d[16]),
                                                                                     double.Parse(d[17]),
                                                                                     double.Parse(d[18]),
                                                                                     double.Parse(d[19]),
                                                                                     double.Parse(d[20]));
                        curBrush.planes.Add(p);
                    }
                }
            }

            if (playerPoss.Count > 0) {
                core.players = new Player[playerPoss.Count];
                for (int i = 0; i < core.players.Length; i++)
                    core.players[i] = new Player(core, PlayerIndex.One + i, playerPoss[i]);
                core.renderEngine = new RenderEngine(core, RenderEngine.Layout.ONE + (playerPoss.Count - 1));
            }

            //Send Lights to shader
            Vector3[] poses = new Vector3[lights.Count];
            Vector4[] colours = new Vector4[lights.Count];
            float[] radii = new float[lights.Count];

            /*for(int i = 0; i < lights.Count; ++i){
            
                poses[i] = lights[i].position;
                colours[i] = lights[i].colour;
                radii[i] = lights[i].radius;
                Console.WriteLine(lights[i].position + " " + lights[i].radius);
            
            }*/

            core.lighting.Parameters["lightPoses"].SetValue(poses);
            core.lighting.Parameters["lightColours"].SetValue(colours);
            core.lighting.Parameters["lightRadii"].SetValue(radii);

        }

        public static Vector3 toWorldSpace(Vector3 gtkradiantSpaceVector) {
            return new Vector3(gtkradiantSpaceVector.X, gtkradiantSpaceVector.Z, -gtkradiantSpaceVector.Y);
        }

        public static int planeSign(Vector3 a, Vector3 b, Vector3 c, Vector3 normal) {
            Vector3 cross = Vector3.Cross(a - b, c - b);
            //project the cross product onto the normal
            if (Vector3.Dot(cross, normal) > 0) //if the dot product is positive, then the projected cross product will point in the same direction as the normal
                return 1;
            else if (Vector3.Dot(cross, normal) < 0)
                return -1;
            return 0;
        }

        public static bool pointOnFace(Vector3 p, Face f) {
            List<Vector3> verts = new List<Vector3>();
            foreach (Vertex v in f.vertices)
                verts.Add(v.position);
            return pointOnFace(p, verts, f.plane.getNormal());
        }

        public static bool pointOnFace(Vector3 p, List<Vector3> faceVertices, Vector3 faceNormal) {
            int sign = planeSign(p, faceVertices[0], faceVertices[1], faceNormal);
            for (int i = 0; i < faceVertices.Count; i++)
                if (planeSign(p, faceVertices[i], faceVertices[(i + 1) % faceVertices.Count], faceNormal) != sign)
                    return false;
            return true;
        }

        public static bool pointInBrush(Vector3 p, Brush b)
        {
            foreach (Face f in b.faces)
            {
                Vector3 c = f.getCenter();
                if (Vector3.Dot(p - c, f.plane.getNormal()) > 0.005f)
                    return false;
            }
            return true;
        }

        public bool pointInBrush(Vector3 point)
        {
            foreach (Brush b in brushes)
                if (pointInBrush(point, b))
                    return true;
            return false;
        }
    }
}
