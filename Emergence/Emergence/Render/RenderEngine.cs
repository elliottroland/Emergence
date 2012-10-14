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
using Emergence.AI;

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

        Dictionary<PlayerIndex, int> playerMap;

        public RenderEngine(CoreEngine c, PlayerIndex[] players)
        {
            core = c;
            Layout l = Layout.ONE + (players.Length - 1);
            curLayout = l;
            ports = new Viewport[(int)l];
            Viewport defaultView = core.GraphicsDevice.Viewport;
            playerMap = new Dictionary<PlayerIndex, int>();
            int index = 0;
            foreach (PlayerIndex pi in players)
                playerMap.Add(pi, index++);

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
            basicEffect.World = Matrix.Identity;

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
            Player p = core.players[playerMap[pi]];
            cameras[playerMap[pi]] = Matrix.CreateLookAt(p.getEyePosition(), p.getEyePosition() + p.getDirectionVector(), Vector3.Up);
        }

        public void drawLine(Vector3 a, Vector3 b, Vector3 colour)
        {

            basicEffect.TextureEnabled = true;
            basicEffect.Texture = core.beamTex;
            basicEffect.Begin();

            //draw the face normal for EVERYTHING!!!
            VertexPositionNormalTexture cent = new VertexPositionNormalTexture(), norm = new VertexPositionNormalTexture();
            cent.Position = a;
            norm.Position = b;
            VertexPositionNormalTexture[] pts = { cent, norm };
            basicEffect.DiffuseColor = colour;
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                //printList(pointList);
                core.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                    PrimitiveType.LineList,
                    pts,
                    0,
                    2,
                    new short[] { 0, 1 },
                    0,
                    1);
                pass.End();
            }
        }

        //Draws the world to each viewport
        public void Draw(GameTime gameTime) {

            int cam = 0;
            foreach (Viewport v in ports) {

                basicEffect.View = cameras[cam];
                //Generate HUD data
                Weapon equipDebug = core.players[cam].equipped;
                
                core.GraphicsDevice.Viewport = v;
                core.GraphicsDevice.Clear(voidColor);
                
                core.GraphicsDevice.VertexDeclaration = vertexDecl;

                //Pass parameters per viewport
                core.lighting.Parameters["World"].SetValue(Matrix.Identity);
                core.lighting.Parameters["View"].SetValue(cameras[cam]);
                core.lighting.Parameters["Projection"].SetValue(basicEffect.Projection);
                core.lighting.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(basicEffect.World)));

                core.lighting.CurrentTechnique = core.lighting.Techniques["Lighting"];
                
                //Draw each brush with effects
                //----------------------------------------------------------------------------------------
                foreach (Brush b in core.mapEngine.brushes)
                    foreach (Face face in b.faces)
                    {
                        core.lighting.Begin();
                        if (face.plane.texture != core.mapEngine.textures["common/caulk"]) {
                            core.lighting.Parameters["Tex"].SetValue(face.plane.texture);
                            foreach (EffectPass pass in core.lighting.CurrentTechnique.Passes) {
                                pass.Begin();
                                VertexPositionNormalTexture[] pointList = face.getPoints();
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

                    }

                //Draw AI info
                //----------------------------------------------------------------------------------------------------

                //basicEffect.Begin();

                /*Vector3 antiLift = new Vector3(0, AIEngine.nodeHeight, 0);
                Vector3 renderLift = new Vector3(0, AIEngine.nodeRenderHeight, 0);
                foreach (MeshNode m in core.aiEngine.mesh) {
                    VertexPositionColor[] line = new VertexPositionColor[2];
                    line[0] = new VertexPositionColor(m.position - antiLift, Color.Red);
                    line[1] = new VertexPositionColor(m.position - antiLift + renderLift, Color.Blue);
                    foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Begin();
                        core.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                            PrimitiveType.LineList,
                            line,
                            0,
                            1);
                        pass.End();

                    }
                }*/

                basicEffect.Begin();
                //draw the collision grid
                /*for(int k = 0; k < core.physicsEngine.grid.GetLength(2); k++)
                    for (int j = 0; j < core.physicsEngine.grid.GetLength(1); j++)
                        for (int i = 0; i < core.physicsEngine.grid.GetLength(0); i++) {
                            if (!core.physicsEngine.grid[i, j, k].elements.Contains(core.getPlayerForIndex(PlayerIndex.One)))
                                continue;
                            Vector3 center = new Vector3((i + 0.5f) * core.physicsEngine.cellSize,
                                                         (j + 0.5f) * core.physicsEngine.cellSize,
                                                         (k + 0.5f) * core.physicsEngine.cellSize);
                            drawLine(new Vector3(i * core.physicsEngine.cellSize, j * core.physicsEngine.cellSize, k * core.physicsEngine.cellSize) + core.physicsEngine.gridOffset,
                                new Vector3((i + 1) * core.physicsEngine.cellSize, j * core.physicsEngine.cellSize, k * core.physicsEngine.cellSize) + core.physicsEngine.gridOffset, new Vector3(0, 1, 0));
                            drawLine(new Vector3(i * core.physicsEngine.cellSize, j * core.physicsEngine.cellSize, k * core.physicsEngine.cellSize) + core.physicsEngine.gridOffset,
                                new Vector3(i * core.physicsEngine.cellSize, j * core.physicsEngine.cellSize, (k + 1) * core.physicsEngine.cellSize) + core.physicsEngine.gridOffset, new Vector3(0, 1, 0));
                            drawLine(new Vector3(i * core.physicsEngine.cellSize, j * core.physicsEngine.cellSize, k * core.physicsEngine.cellSize) + core.physicsEngine.gridOffset,
                                new Vector3(i * core.physicsEngine.cellSize, (j + 1) * core.physicsEngine.cellSize, k * core.physicsEngine.cellSize) + core.physicsEngine.gridOffset, new Vector3(0, 1, 0));
                        }*/

               // basicEffect.End();

                //draw the ai agents
                foreach (AIAgent a in core.aiEngine.agents)
                {
                    BoundingBox bb = a.getBoundingBox();
                    Vector3 size = bb.Max - bb.Min;
                    Vector3 sizeX = new Vector3(size.X, 0, 0),
                            sizeY = new Vector3(0, size.Y, 0),
                            sizeZ = new Vector3(0, 0, size.Z);
                    drawLine(bb.Min, bb.Min + sizeY, new Vector3(1, 0, 0));
                    drawLine(bb.Min, bb.Min + sizeZ, new Vector3(1, 0, 0));
                    drawLine(bb.Min, bb.Min + sizeX, new Vector3(1, 0, 0));
                    drawLine(bb.Min + sizeZ, bb.Min + sizeZ + sizeX, new Vector3(1, 0, 0));
                    drawLine(bb.Min + sizeX, bb.Min + sizeZ + sizeX, new Vector3(1, 0, 0));
                    drawLine(bb.Min + sizeZ, bb.Min + sizeZ + sizeY, new Vector3(1, 0, 0));
                    drawLine(bb.Min + sizeX, bb.Min + sizeY + sizeX, new Vector3(1, 0, 0));
                    drawLine(bb.Max, bb.Max - sizeY, new Vector3(1, 0, 0));
                    drawLine(bb.Max, bb.Max - sizeZ, new Vector3(1, 0, 0));
                    drawLine(bb.Max, bb.Max - sizeX, new Vector3(1, 0, 0));
                    drawLine(bb.Max - sizeZ, bb.Max - sizeZ - sizeX, new Vector3(1, 0, 0));
                    drawLine(bb.Max - sizeX, bb.Max - sizeX - sizeZ, new Vector3(1, 0, 0));

                    //also draw the path
                    MeshNode last = null;
                    foreach (MeshNode m in a.path)
                    {
                        if (last == null)
                        {
                            last = m;
                            continue;
                        }
                        drawLine(m.position, last.position, new Vector3(1, 0, 0));
                        last = m;
                    }
                }

                basicEffect.End();

                //Draw each players bullets
                //--------------------------------------------------------------------------------------------
                core.lighting.CurrentTechnique = core.lighting.Techniques["Texturing"];
                core.lighting.Parameters["Tex"].SetValue(core.bulletTex);
                foreach(Player p in core.players){

                    VertexPositionColor[,] bVerts = new VertexPositionColor[p.bullets.Count, 1];
                    int i = 0;

                    foreach (Bullet b in p.bullets) {

                        bVerts[i++,0].Position = b.pos;
                    
                    }
                    
                    //If bullets exist for the player
                    if (bVerts.Length > 0)
                    {

                        for (int j = 0; j < bVerts.Length; ++j )
                        {
                            VertexPositionColor[] point = new VertexPositionColor[1];
                            point[0] = bVerts[j,0];

                            core.lighting.Begin();
                            core.GraphicsDevice.RenderState.PointSpriteEnable = true;
                            core.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                            core.GraphicsDevice.RenderState.SourceBlend = Blend.One;
                            core.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
                            core.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

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

                    }
                
                }

                core.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                core.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                core.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                core.GraphicsDevice.RenderState.AlphaTestEnable = true;
                core.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

                core.lighting.CurrentTechnique = core.lighting.Techniques["FadeTexturing"];
                core.lighting.Parameters["Tex"].SetValue(core.beamTex);

                foreach (Player p in core.players)
                {
                    foreach (Laser l in p.lasers)
                    {
                        core.lighting.Parameters["Opacity"].SetValue(l.timeLeft/600f);
                        core.lighting.Begin();
                        foreach (EffectPass pass in core.lighting.CurrentTechnique.Passes)
                        {

                            pass.Begin();
                            core.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                                            PrimitiveType.TriangleList,
                                            l.horizVerts,
                                            0,
                                            4,
                                            l.indices,
                                            0,
                                            2);
                            core.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                                            PrimitiveType.TriangleList,
                                            l.horizVerts,
                                            0,
                                            4,
                                            l.revIndices,
                                            0,
                                            2);
                            core.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                                            PrimitiveType.TriangleList,
                                            l.vertVerts,
                                            0,
                                            4,
                                            l.indices,
                                            0,
                                            2);
                            core.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                                            PrimitiveType.TriangleList,
                                            l.vertVerts,
                                            0,
                                            4,
                                            l.revIndices,
                                            0,
                                            2);

                            pass.End();

                        }
                        core.lighting.End();
                    }
                    
                }

                core.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                core.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                core.GraphicsDevice.RenderState.SourceBlend = Blend.One;
                core.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                core.GraphicsDevice.RenderState.AlphaTestEnable = false;

                //Draw item spawner locations
                //----------------------------------------------------------------------------------------------------
                foreach (PickUpGen gen in core.pickupEngine.gens)
                {
                    foreach (ModelMesh mesh in core.debugSphere.Meshes)
                    {

                        foreach (BasicEffect effect in mesh.Effects)
                        {

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

                        Model pickUpModel = core.debugSphere;

                        switch (gen.itemType)
                        {

                            case PickUp.PickUpType.AMMO: pickUpModel = core.ammoUp; break;
                            case PickUp.PickUpType.HEALTH: pickUpModel = core.medicross; break;
                            default: pickUpModel = core.arrow; break;

                        }

                        foreach (ModelMesh mesh in pickUpModel.Meshes)
                        {

                            foreach (BasicEffect effect in mesh.Effects)
                            {
                                effect.World = Matrix.CreateScale(5) * Matrix.CreateRotationY(gen.held.rotation) * Matrix.CreateTranslation(gen.held.pos);
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

                //Draw HUD

                /*core.DrawStringDebug("" + equipDebug.GetType()
                                + "\nCooldown: " + equipDebug.curCooldown + "/" + equipDebug.cooldown

                                + "\nAmmo: " + core.players[cam].ammo);
                */

                cam++;
            
            }
        
        }        

    }

}