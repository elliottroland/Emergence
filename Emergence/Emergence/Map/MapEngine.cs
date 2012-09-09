using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.RegularExpressions;

using Emergence.Render;

namespace Emergence.Map {
    public class MapEngine {
        public Dictionary<String, Texture2D> textures;
        public List<Brush> brushes;
        CoreEngine core;

        public MapEngine(CoreEngine c, String filename, Dictionary<String, Texture2D> textures) {
            core = c;
            this.textures = textures;
            brushes = new List<Brush>();
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
        }

        public static Vector3 toWorldSpace(Vector3 gtkradiantSpaceVector) {
            return new Vector3(gtkradiantSpaceVector.X, gtkradiantSpaceVector.Z, -gtkradiantSpaceVector.Y);
        }
    }
}
