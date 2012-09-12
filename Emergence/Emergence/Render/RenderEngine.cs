//Class for rendering the world to each players screen
//Daanyaal du Toit
//30 August 2012

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using Emergence.Map;
using Emergence.Weapons;

namespace Emergence.Render
{
    /// <summary>
    /// Handles viewports based on player number
    /// </summary>
    public class RenderEngine
    {
        CoreEngine core;
        //Standard aspect ratio projection
        Matrix fullMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 4.0f / 3.0f, 1f, 10000f);

        //Half screen aspect ratio projection (for two players)
        Matrix halfMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 2.0f / 3.0f, 1f, 10000f);

        //Layout determined by number of human players
        public enum Layout {ONE=1, TWO, THREE, FOUR};
        private Layout curLayout;
        private Viewport[] ports;

        //for rendering worldspawn
        public BasicEffect basicEffect;
        public VertexDeclaration vertexDecl;

        public Color voidColor = Color.Black;

        Matrix [] cameras;

        public RenderEngine(CoreEngine c, Layout l)
        {
            core = c;
            curLayout = l;
            ports = new Viewport[(int)l];
            Viewport defaultView = core.GraphicsDevice.Viewport;

            cameras = new Matrix[(int)l];

            //Single player
            if (l == Layout.ONE) {
                
                ports[0] = defaultView;
            
            }
            //Two players
            else if (l == Layout.TWO) {

                ports[0] = defaultView;
                ports[0].Width /= 2;

                ports[1] = ports[0];
                ports[1].X += ports[0].Width;
            
            }
            //Three players
            else if (l == Layout.THREE)
            {

                ports[0] = defaultView;
                ports[0].Width /= 2;
                ports[0].Height /= 2;

                ports[1] = ports[0];
                ports[1].X += ports[0].Width;

                ports[2] = ports[0];
                ports[2].X += ports[2].Width / 2;
                ports[2].Y += ports[2].Height;

            }
            //Four players
            else {

                ports[0] = defaultView;
                ports[0].Width /= 2;
                ports[0].Height /= 2;

                ports[1] = ports[0];
                ports[1].X += ports[0].Width;

                ports[2] = ports[0];
                ports[2].Y += ports[0].Height;

                ports[3] = ports[1];
                ports[3].Y += ports[1].Height;
            
            }

            //initialise the basicEffect and vertex declaration
            vertexDecl = new VertexDeclaration(
                core.GraphicsDevice,
                VertexPositionNormalTexture.VertexElements);

            basicEffect = new BasicEffect(core.GraphicsDevice, null);
            basicEffect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);

            //basicEffect.View = viewMatrix;
            if(curLayout == Layout.TWO)
                basicEffect.Projection = halfMatrix;
            else
                basicEffect.Projection = fullMatrix;
        }

        //Returns the viewport (not needed yet, if at all)
        /*public Viewport[] getViews(){
            return ports;
        }*/

        public void updateCameraForPlayer(PlayerIndex pi)   {
            Player p = core.players[(int)pi];
            cameras[(int)pi] = Matrix.CreateLookAt(p.getEyePosition(), p.getEyePosition() + p.getDirectionVector(), Vector3.Up);
        }

        //Draws the world to each viewport
        //The passed model and font are for debugging
        public void Draw(GameTime gameTime) {
            int cam = 0;
            foreach (Viewport v in ports) {

                core.GraphicsDevice.Viewport = v;
                core.GraphicsDevice.Clear(voidColor);

                core.GraphicsDevice.VertexDeclaration = vertexDecl;
                basicEffect.View = cameras[cam];
                basicEffect.World = Matrix.Identity;
                basicEffect.TextureEnabled = true;

                foreach (Brush b in core.mapEngine.brushes)
                    foreach (Face face in b.faces)
                    {
                        basicEffect.Begin();
                        if (face.plane.texture != core.mapEngine.textures["common/caulk"]) {
                            basicEffect.Texture = face.plane.texture;
                            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
                                pass.Begin();
                                VertexPositionNormalTexture[] pointList = face.getPoints();
                                //printList(pointList);
                                short[] indices = new short[pointList.Length];
                                for (int i = 0; i < pointList.Length; i++)
                                    indices[i] = (short)i;
                                core.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                                    PrimitiveType.TriangleFan,
                                    pointList,
                                    0,
                                    pointList.Length,
                                    indices,
                                    0,
                                    pointList.Length - 2);
                                pass.End();
                            }
                        }

                        basicEffect.End();
                    }

                Weapon equipDebug = core.players[cam].equipped;

                core.DrawTextDebug(""+equipDebug.GetType()
                                + "\nCooldown: " + equipDebug.curCooldown + "/" + equipDebug.cooldown
                                + "\nAmmo: " + core.players[cam].ammo);

                cam++;
            
            }
        
        }        

    }

}