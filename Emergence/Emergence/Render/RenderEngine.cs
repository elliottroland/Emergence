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
using Emergence.Pickup;

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

        float testrot = 0;          //0 to 2PI

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

                //Pass parameters per viewport
                core.lighting.Parameters["World"].SetValue(Matrix.Identity);
                core.lighting.Parameters["View"].SetValue(cameras[cam]);
                core.lighting.Parameters["Projection"].SetValue(basicEffect.Projection);
                core.lighting.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(basicEffect.World)));
                core.lighting.Parameters["DifDir"].SetValue(new Vector3(1*(float)Math.Sin(testrot), 0, 1*(float)Math.Cos(testrot)));

                //so I can see
                core.lighting.Parameters["Ambient"].SetValue(new Vector4(1,1,1,1));

                //Draw each brush with effects
                //----------------------------------------------------------------------------------------
                foreach (Brush b in core.mapEngine.brushes)
                    foreach (Face face in b.faces)
                    {
                        core.lighting.Begin();
                        if (face.plane.texture != core.mapEngine.textures["common/caulk"]) {
                            /*basicEffect.Texture = face.plane.texture;
                            //basicEffect.DiffuseColor = new Vector3(1, 1, 1);
                            basicEffect.DiffuseColor = face.DiffuseColor;
                            if (face.DiffuseColor == new Vector3(1,1,1) && b.colliding)
                                basicEffect.DiffuseColor = new Vector3(1, 0, 0);*/
                            core.lighting.Parameters["tex"].SetValue(face.plane.texture);
                            //foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
                            foreach (EffectPass pass in core.lighting.CurrentTechnique.Passes) {
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

                        core.lighting.End();
                        //draw the face normal
                        /*VertexPositionNormalTexture cent = new VertexPositionNormalTexture(), norm = new VertexPositionNormalTexture();
                        cent.Position = face.getCenter();
                        norm.Position = face.plane.getNormal() * 60 + cent.Position;
                        VertexPositionNormalTexture[] pts = { cent, norm };
                        basicEffect.Texture = null;
                        basicEffect.DiffuseColor = new Vector3(1, 0, 0);
                        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes) {
                            pass.Begin();
                            //printList(pointList);
                            core.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                                PrimitiveType.LineList,
                                pts,
                                0,
                                2,
                                new short[] {0, 1},
                                0,
                                1);
                            pass.End();
                        }*/


                        //basicEffect.End();
                    }

                //----------------------------------------------------------------------------------------------------

                //Draw item spawner locations
                //----------------------------------------------------------------------------------------------------
                foreach (PickUpGen gen in core.pickupEngine.gens)
                {
                    foreach(ModelMesh mesh in core.debugSphere.Meshes){
                    
                        foreach(BasicEffect effect in mesh.Effects){
                        
                            effect.World = Matrix.CreateScale(10) * Matrix.CreateTranslation(gen.pos);
                            effect.View = cameras[cam];
                            effect.Projection = basicEffect.Projection;

                            effect.LightingEnabled = true;
                            effect.AmbientLightColor = Color.Brown.ToVector3();

                        }

                        mesh.Draw();
                    
                    }

                    if (gen.held != null)
                    {

                        foreach (ModelMesh mesh in core.debugSphere.Meshes)
                        {

                            foreach (BasicEffect effect in mesh.Effects)
                            {
                                effect.World = Matrix.CreateScale(10) * Matrix.CreateRotationY(gen.held.rotation) * Matrix.CreateTranslation(gen.held.pos);
                                effect.View = cameras[cam];
                                effect.Projection = basicEffect.Projection;
                                effect.LightingEnabled = true;
                                
                                switch (gen.held.type)
                                {

                                    case PickUp.PickUpType.AMMO: { effect.AmbientLightColor = Color.Crimson.ToVector3(); break; }
                                    case PickUp.PickUpType.HEALTH: { effect.AmbientLightColor = Color.Green.ToVector3(); break; }
                                    case PickUp.PickUpType.LEFT: { effect.AmbientLightColor = Color.DarkBlue.ToVector3(); break; }
                                    case PickUp.PickUpType.RIGHT: { effect.AmbientLightColor = Color.Yellow.ToVector3(); break; }

                                }

                            }

                            mesh.Draw();

                        }

                    }
                }

                //--------------------------------------------------------------------------------------------

                //Draw each players bullets
                //--------------------------------------------------------------------------------------------
                foreach(Player p in core.players){

                    VertexPositionColor[,] bVerts = new VertexPositionColor[p.bullets.Count, 1];
                    int i = 0;

                    foreach (Bullet b in p.bullets) {

                        bVerts[i++,0].Position = b.pos;
                    
                    }
                    
                    //If bullets exist for the player
                    if (bVerts.Length > 0)
                    {
                        core.GraphicsDevice.RenderState.PointSpriteEnable = true;
                        core.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        core.GraphicsDevice.RenderState.SourceBlend = Blend.One;
                        core.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
                        core.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
                        core.lighting.Parameters["tex"].SetValue(core.bulletTex);
                        core.lighting.Parameters["Diffuse"].SetValue(new Vector4());

                        for (int j = 0; j < bVerts.Length; ++j )
                        {
                            VertexPositionColor[] point = new VertexPositionColor[1];
                            point[0] = bVerts[j,0];

                            core.lighting.Begin();

                            core.GraphicsDevice.RenderState.PointSize = (64f * 100) / Vector3.Distance(p.position, point[0].Position);
                            foreach (EffectPass pass in core.lighting.CurrentTechnique.Passes)
                            {
                                pass.Begin();
                                core.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                                    PrimitiveType.PointList,
                                    point,
                                    0,
                                    1);
                                pass.End();
                            }

                            core.lighting.End();

                        }
                        core.GraphicsDevice.RenderState.PointSpriteEnable = false;
                        core.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                        core.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                        core.GraphicsDevice.RenderState.SourceBlend = Blend.One;
                        core.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                        core.lighting.Parameters["Diffuse"].SetValue(new Vector4(0,1,0,1));
                    }
                
                }

                //Weapon equipDebug = core.players[cam].equipped;

                /*core.DrawTextDebug(""+equipDebug.GetType()
                                + "\nCooldown: " + equipDebug.curCooldown + "/" + equipDebug.cooldown

                                + "\nAmmo: " + core.players[cam].ammo);*/

                cam++;
                //testrot += 0.1f;
                //if (testrot > 2 * Math.PI)
                   // testrot -= 2 * (float)Math.PI;
            
            }
        
        }        

    }

}