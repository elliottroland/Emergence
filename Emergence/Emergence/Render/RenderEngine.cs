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

namespace Emergence.Render {

    //Weapon helpers
    //--------------------------------------------------------------------------------------------------

    public struct Bullet {

        public Vector3 pos;
        public Vector3 dir;
        public int timeLeft;

        public void update() {

            pos = pos + dir * 50;
            timeLeft -= 10;

        }

    }

    public struct Laser {

        public VertexPositionNormalTexture[] horizVerts;
        public VertexPositionNormalTexture[] vertVerts;
        public int[] indices;
        public int[] revIndices;
        public int timeLeft;
        public Texture2D texture;

        public void update() {
            timeLeft -= 10;
        }

    }

    public class Projectile {
        public float size, speed, damage, explosionSize;
        public Vector3 position, dir;
        public VertexPositionNormalTexture[] a, b, c;
        public int[] indices;
        public int[] revIndices;
        public float collisionDist;
        public Texture2D texture, explosionTexture;
        public Agent p;

        public Projectile() { }

        public void update(GameTime gameTime) {
            Vector3 move = dir * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            position += move;
            collisionDist -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //update the vertices
            for (int i = 0; i < a.Length; i++) a[i].Position += move;
            for (int i = 0; i < b.Length; i++) b[i].Position += move;
            for (int i = 0; i < c.Length; i++) c[i].Position += move;
        }
    }

    public class Explosion : Projectile {
        public void update2(GameTime gameTime) {
            size += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            collisionDist -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Console.WriteLine(collisionDist);

            for (int i = 0; i < a.Length; i++)
                a[i].Position = Vector3.Normalize(a[i].Position - position) * size + position;
            for (int i = 0; i < b.Length; i++)
                b[i].Position = Vector3.Normalize(b[i].Position - position) * size + position;
            for (int i = 0; i < c.Length; i++)
                c[i].Position = Vector3.Normalize(c[i].Position - position) * size + position;
        }
    }

    public class Rocket {
        public float speed, size, trailDist, curTrailDist, collisionDist, damage, explosionSize;
        public Vector3 position, dir;
        public Agent p;
        public Texture2D explosionTexture;

        public Rocket() { }

        public void update(GameTime gameTime) {
            Vector3 move = dir * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            position += move;
            curTrailDist += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            collisionDist -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (curTrailDist > trailDist) {
                Weapon.makeLaser(p, new Ray(position, -dir), curTrailDist, 20, 20, "RocketLauncher");
                curTrailDist = 0;
            }
        }
    }

    //------------------------------------------------------------------------------------------------------------------------


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
        private BoundingFrustum[] frustums;

        //for rendering worldspawn
        public BasicEffect basicEffect;
        public VertexDeclaration vertexDecl;

        public Color voidColor = Color.Black;

        Matrix [] cameras;

        Dictionary<PlayerIndex, int> playerMap;
        public static Viewport defaultView;

        public List<Laser> lasers;
        public List<Projectile> projectiles;
        public List<Rocket> rockets;
        public List<Explosion> explosions;

        public IEnumerable<Projectile> projectilesAndExplosions() {
            foreach (Projectile p in projectiles)
                yield return p;
            foreach (Projectile p in explosions)
                yield return p;
        }

        public RenderEngine(CoreEngine c, PlayerIndex[] players)
        {
            core = c;
            lasers = new List<Laser>();
            projectiles = new List<Projectile>();
            explosions = new List<Explosion>();
            rockets = new List<Rocket>();
            Layout l = Layout.ONE + (players.Length - 1);
            curLayout = l;
            ports = new Viewport[(int)l];
            //defaultView = core.GraphicsDevice.Viewport;
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

            frustums = new BoundingFrustum[(int)curLayout];
            for (int i = 0; i < (int)curLayout; ++i)
                frustums[i] = new BoundingFrustum(Matrix.Identity);

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

        public void drawBoundingBox(BoundingBox bb, Vector3 colour) {
            Vector3 size = bb.Max - bb.Min;
            Vector3 sizeX = new Vector3(size.X, 0, 0),
                    sizeY = new Vector3(0, size.Y, 0),
                    sizeZ = new Vector3(0, 0, size.Z);
            drawLine(bb.Min, bb.Min + sizeY, colour);
            drawLine(bb.Min, bb.Min + sizeZ, colour);
            drawLine(bb.Min, bb.Min + sizeX, colour);
            drawLine(bb.Min + sizeZ, bb.Min + sizeZ + sizeX, colour);
            drawLine(bb.Min + sizeX, bb.Min + sizeZ + sizeX, colour);
            drawLine(bb.Min + sizeZ, bb.Min + sizeZ + sizeY, colour);
            drawLine(bb.Min + sizeX, bb.Min + sizeY + sizeX, colour);
            drawLine(bb.Max, bb.Max - sizeY, colour);
            drawLine(bb.Max, bb.Max - sizeZ, colour);
            drawLine(bb.Max, bb.Max - sizeX, colour);
            drawLine(bb.Max - sizeZ, bb.Max - sizeZ - sizeX, colour);
            drawLine(bb.Max - sizeX, bb.Max - sizeX - sizeZ, colour);
        }

        //Draws the world to each viewport
        public void Draw(GameTime gameTime) {

            int cam = 0;
            foreach (Viewport v in ports) {

                basicEffect.View = cameras[cam];

                frustums[cam].Matrix = basicEffect.View * basicEffect.Projection;

                ContainmentType currentContainmentType = ContainmentType.Disjoint;

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
                foreach (Brush b in core.mapEngine.brushes) {

                    BoundingBox bb = b.boundingBox;
                    currentContainmentType = frustums[cam].Contains(bb);

                    if (currentContainmentType != ContainmentType.Disjoint)
                        foreach (Face face in b.faces) {
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
                }

                //Draw AI info
                //----------------------------------------------------------------------------------------------------
                
                    basicEffect.Begin();

                    /*Vector3 antiLift = new Vector3(0, AIEngine.nodeHeight, 0);
                    Vector3 renderLift = new Vector3(0, AIEngine.nodeRenderHeight, 0);
                    foreach (MeshNode m in core.aiEngine.mesh) {
                        drawLine(m.position, m.position, new Vector3(1, 1, 1));
                        foreach(MeshNode m2 in m.neighbours)
                            drawLine(m2.position, m.position, new Vector3(1, 1, 1));
                    }*/

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
                    /*foreach (AIAgent a in core.aiEngine.agents) {

                        if (a.spawnTime > 0) continue;
                        BoundingBox bb = a.getBoundingBox();
                        currentContainmentType = frustums[cam].Contains(bb);
                        if (currentContainmentType != ContainmentType.Disjoint)
                        drawBoundingBox(bb, new Vector3(1, 0, 0));

                        //also draw the path
                        MeshNode last = null;
                        foreach (MeshNode m in a.path) {
                            if (last == null) {
                                last = m;
                                continue;
                            }
                            drawLine(m.position, last.position, new Vector3(1, 0, 0));
                            last = m;
                        }
                    }

                    foreach (Player p in core.players) {

                        BoundingBox bb = p.getBoundingBox();
                        currentContainmentType = frustums[cam].Contains(bb);

                        if (p.spawnTime > 0) continue;
                        if (currentContainmentType != ContainmentType.Disjoint)
                            drawBoundingBox(p.getBoundingBox(), new Vector3(0, 1, 0));
                        foreach (AIAgent a in core.aiEngine.agents)
                            drawLine(p.getPosition() + new Vector3(0, 32, 0), a.getPosition() + new Vector3(0, 32, 0), new Vector3(0, 0, 1));
                    }*/

                    basicEffect.End();
                
                
                

                //Draw each players bullets
                //--------------------------------------------------------------------------------------------
                /*core.lighting.CurrentTechnique = core.lighting.Techniques["Texturing"];
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
                }*/

                core.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                core.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                core.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                core.GraphicsDevice.RenderState.AlphaTestEnable = true;
                core.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

                core.lighting.CurrentTechnique = core.lighting.Techniques["FadeTexturing"];

                foreach (Laser l in lasers) {
                    if (l.texture == null) continue;
                    core.lighting.Parameters["Tex"].SetValue(l.texture);
                    core.lighting.Parameters["Opacity"].SetValue(l.timeLeft / 600f);
                    core.lighting.Begin();
                    foreach (EffectPass pass in core.lighting.CurrentTechnique.Passes) {

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
                core.lighting.Parameters["Opacity"].SetValue(1);
                foreach (Projectile l in projectilesAndExplosions()) {
                    if (l.texture == null) continue;
                    core.lighting.Parameters["Tex"].SetValue(l.texture);
                    core.lighting.Begin();
                    foreach (EffectPass pass in core.lighting.CurrentTechnique.Passes) {

                        pass.Begin();
                        core.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                                        PrimitiveType.TriangleList,
                                        l.a,
                                        0,
                                        4,
                                        l.indices,
                                        0,
                                        2);
                        core.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                                        PrimitiveType.TriangleList,
                                        l.a,
                                        0,
                                        4,
                                        l.revIndices,
                                        0,
                                        2);
                        core.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                                        PrimitiveType.TriangleList,
                                        l.b,
                                        0,
                                        4,
                                        l.indices,
                                        0,
                                        2);
                        core.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                                        PrimitiveType.TriangleList,
                                        l.b,
                                        0,
                                        4,
                                        l.revIndices,
                                        0,
                                        2);
                        core.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                                        PrimitiveType.TriangleList,
                                        l.c,
                                        0,
                                        4,
                                        l.indices,
                                        0,
                                        2);
                        core.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                                        PrimitiveType.TriangleList,
                                        l.c,
                                        0,
                                        4,
                                        l.revIndices,
                                        0,
                                        2);

                        pass.End();

                    }
                    core.lighting.End();
                }

                core.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                core.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                core.GraphicsDevice.RenderState.SourceBlend = Blend.One;
                core.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                core.GraphicsDevice.RenderState.AlphaTestEnable = false;

                //Draw item spawner locations
                //----------------------------------------------------------------------------------------------------
                foreach (PickUpGen gen in core.pickupEngine.gens) {
                    /*foreach (ModelMesh mesh in core.debugSphere.Meshes) {

                        foreach (BasicEffect effect in mesh.Effects) {

                            effect.World = Matrix.CreateScale(10) * Matrix.CreateTranslation(gen.pos);
                            effect.View = cameras[cam];
                            effect.Projection = basicEffect.Projection;

                            effect.LightingEnabled = true;
                            effect.AmbientLightColor = Color.Brown.ToVector3();

                        }

                        mesh.Draw();

                    }*/

                    if (gen.held != null) {

                        Model pickUpModel = core.arrow;

                        switch (gen.itemType) {

                            case PickUp.PickUpType.AMMO: pickUpModel = core.ammoUp; break;
                            case PickUp.PickUpType.HEALTH: pickUpModel = core.medicross; break;

                        }   

                        foreach (ModelMesh mesh in pickUpModel.Meshes) {

                            foreach (BasicEffect effect in mesh.Effects) {
                                effect.World = Matrix.CreateScale(5) * Matrix.CreateRotationY(gen.held.rotation) * Matrix.CreateTranslation(gen.held.pos);
                                effect.View = cameras[cam];
                                effect.Projection = basicEffect.Projection;
                                effect.LightingEnabled = true;

                                switch (gen.held.type) {

                                    case PickUp.PickUpType.AMMO: { effect.AmbientLightColor = Color.Yellow.ToVector3(); break; }
                                    case PickUp.PickUpType.HEALTH: { effect.AmbientLightColor = Color.Green.ToVector3(); break; }
                                    case PickUp.PickUpType.LEFT: { effect.AmbientLightColor = Color.DarkBlue.ToVector3(); break; }
                                    case PickUp.PickUpType.RIGHT: { effect.AmbientLightColor = Color.Red.ToVector3(); break; }

                                }

                            }

                            mesh.Draw();

                        }

                    }
                }

                //draw rockets
                foreach (Rocket gen in rockets) {
                    foreach (ModelMesh mesh in core.debugSphere.Meshes) {

                        foreach (BasicEffect effect in mesh.Effects) {

                            effect.World = Matrix.CreateScale(10) * Matrix.CreateTranslation(gen.position);
                            effect.View = cameras[cam];
                            effect.Projection = basicEffect.Projection;

                            effect.LightingEnabled = true;
                            effect.AmbientLightColor = Color.Brown.ToVector3();

                        }

                        mesh.Draw();
                    }
                }

                //draw explosions


                //--------------------------------------------------------------------------------------------

                //--------------------------------------------------------------------------------------------

                foreach (AIAgent ai in core.aiEngine.agents)
                    if(ai.spawnTime == 0)
                    foreach (ModelMesh mesh in core.steve.Meshes) {
                        foreach (BasicEffect effect in mesh.Effects) {

                            effect.World = Matrix.CreateScale(1.5f) * Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2 - ai.direction.X) * Matrix.CreateTranslation(ai.getPosition() + new Vector3(0, core.steve.Meshes[0].BoundingSphere.Radius * 1.5f, 0));
                            effect.View = cameras[cam];
                            effect.Projection = basicEffect.Projection;

                            //effect.LightingEnabled = true;
                            //effect.AmbientLightColor = Color.Brown.ToVector3();

                        }
                        mesh.Draw();
                    }

                foreach (Player p in core.players) { 
                
                    if(playerMap[p.playerIndex] != cam)
                        foreach (ModelMesh mesh in core.steve.Meshes) {

                            foreach (BasicEffect effect in mesh.Effects) {

                                effect.World = Matrix.CreateScale(1.5f) * Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2 - p.direction.X) * Matrix.CreateTranslation(p.getPosition() + new Vector3(0, core.steve.Meshes[0].BoundingSphere.Radius * 1.5f, 0));
                                effect.View = cameras[cam];
                                effect.Projection = basicEffect.Projection;

                                //effect.LightingEnabled = true;
                                //effect.AmbientLightColor = Color.Brown.ToVector3();

                            }

                            mesh.Draw();
                        }

                
                }

                Player player = core.players[cam];
                Texture2D weaponIcon = core.weaponIcons[player.equipped.GetType()];
                core.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);

                Vector2 screenCenter = new Vector2(v.Width / 2, v.Height / 2);

                Vector2 ammoPos = new Vector2(screenCenter.X, v.Height - 80);
                core.spriteBatch.Draw(weaponIcon, ammoPos, Color.White);
                core.spriteBatch.DrawString(core.debugFont, player.ammo.ToString(), new Vector2(ammoPos.X + weaponIcon.Width, ammoPos.Y + weaponIcon.Height / 3), Color.White);


                Vector2 hpPos = new Vector2(screenCenter.X, v.Height - 80);
                core.spriteBatch.Draw(core.hudIcons["Health"], hpPos -= new Vector2(core.hudIcons["Health"].Width, 0), Color.White);
                core.spriteBatch.DrawString(core.debugFont, player.health.ToString(), new Vector2(hpPos.X - core.debugFont.MeasureString(player.health.ToString()).X, hpPos.Y + weaponIcon.Height / 3), Color.White);


                String mins = "" + Math.Floor(core.roundTime / 60);
                String secs = "" + Math.Round(core.roundTime % 60);
                core.spriteBatch.DrawString(core.debugFont, mins + ":" + secs, new Vector2(screenCenter.X, 80) - (core.debugFont.MeasureString(mins + ":" + secs) / 2), Color.White);

                core.spriteBatch.Draw(MenuScreen.crossHair, screenCenter, new Rectangle(0, 0, MenuScreen.crossHair.Width, MenuScreen.crossHair.Height),
                new Color(new Vector4(Color.White.ToVector3(),0.65f)),
                0f, new Vector2(MenuScreen.crossHair.Width / 2, MenuScreen.crossHair.Height / 2 + 25),
                0.05f, SpriteEffects.None, 0);


                if (core.players[cam].drawUpgradePath) {
                    //Texture2D downGradeTexture = MenuScreen.selectWheel;
                    Vector2 wheelCenter = new Vector2(screenCenter.X, v.Height - player.currentPathsWheelHeight + MenuScreen.selectWheel.Height / 2);

                    core.spriteBatch.Draw(MenuScreen.loadWheel, wheelCenter, new Rectangle(0, 0, MenuScreen.loadWheel.Width, MenuScreen.loadWheel.Height),
                        Color.White, MathHelper.PiOver4, new Vector2(MenuScreen.loadWheel.Width / 2, MenuScreen.loadWheel.Height / 2 + 25),
                        0.5f, SpriteEffects.None, 0);

                    core.spriteBatch.Draw(MenuScreen.ugLeft, wheelCenter, new Rectangle(0, 0, MenuScreen.ugLeft.Width, MenuScreen.ugLeft.Height),
                        Color.White, -MathHelper.PiOver4, new Vector2(MenuScreen.ugLeft.Width / 2, MenuScreen.ugLeft.Height / 2 + 25),
                        0.5f, SpriteEffects.None, 0);

                    core.spriteBatch.Draw(MenuScreen.ugRight, wheelCenter, new Rectangle(0, 0, MenuScreen.ugRight.Width, MenuScreen.ugRight.Height),
                        Color.White, MathHelper.PiOver4, new Vector2(MenuScreen.ugRight.Width / 2, MenuScreen.ugRight.Height / 2 + 25),
                        0.5f, SpriteEffects.None, 0);

                    Texture2D leftWeaponIcon = core.weaponIcons[player.equipped.upgradeLeft().GetType()];
                    Texture2D rightWeaponIcon = core.weaponIcons[player.equipped.upgradeRight().GetType()];

                    Vector2 rightPos = new Vector2(wheelCenter.X + v.Width / 4, wheelCenter.Y - 280);
                    core.spriteBatch.Draw(rightWeaponIcon, rightPos - new Vector2(rightWeaponIcon.Width, 0), Color.White);
                    String right = player.equipped.upgradeRight().GetType().ToString();
                    core.spriteBatch.DrawString(core.debugFont, right.Substring(right.LastIndexOf(".") + 1), new Vector2(rightPos.X, rightPos.Y + rightWeaponIcon.Height / 3), Color.White);

                    Vector2 leftPos = new Vector2(wheelCenter.X - v.Width / 4, wheelCenter.Y - 280);
                    core.spriteBatch.Draw(leftWeaponIcon, leftPos, Color.White);
                    String left = player.equipped.upgradeLeft().GetType().ToString();
                    core.spriteBatch.DrawString(core.debugFont, left.Substring(left.LastIndexOf(".") + 1), new Vector2(leftPos.X - core.debugFont.MeasureString(left.Substring(left.LastIndexOf(".") + 1)).X, leftPos.Y + leftWeaponIcon.Height / 3), Color.White);


                }

                if (core.players[cam].showScoreboard) {
                    core.spriteBatch.Draw(MenuScreen.loadWheel, screenCenter, new Rectangle(0, 0, MenuScreen.loadWheel.Width, MenuScreen.loadWheel.Height),
                       Color.White, 0f, new Vector2(MenuScreen.loadWheel.Width / 2, MenuScreen.loadWheel.Height / 2 + 25),
                       0.5f, SpriteEffects.None, 1f);

                }

                if (core.players[cam].damageAlpha1 != 0 || core.players[cam].damageAlpha2 != 0) {
                    core.spriteBatch.Draw(MenuScreen.splatter1,
                        new Rectangle((int)screenCenter.X - v.Width / 2, (int)screenCenter.Y - v.Height / 2, v.Width, v.Height),
                        new Rectangle(0, 0, 1024, 576),
                       new Color((new Vector4(Color.White.ToVector3(), core.players[cam].damageAlpha1)))
                           , 0f, Vector2.Zero,
                       SpriteEffects.None, 1f);

                    core.spriteBatch.Draw(MenuScreen.splatter2,
                        new Rectangle((int)screenCenter.X - v.Width / 2, (int)screenCenter.Y - v.Height / 2, v.Width, v.Height),
                        new Rectangle(0, 0, 1024, 576),
                       new Color((new Vector4(Color.White.ToVector3(), core.players[cam].damageAlpha2)))
                           , 0f, Vector2.Zero,
                       SpriteEffects.None, 1f);

                }

                core.spriteBatch.End();
                cam++;

            }

            }
        
        public void Update(GameTime gameTime) {
            //update all bullet positions
            /*for (int i = bullets.Count - 1; i >= 0; --i) {
                Bullet curB = bullets[i];
                curB.update();
                bullets[i] = curB;
                if (bullets[i].timeLeft <= 0)
                    bullets.Remove(bullets[i]);

            }*/

            for (int i = 0; i < rockets.Count; ++i) {
                rockets[i].update(gameTime);
                Vector3 radius = new Vector3(1,1,1) * rockets[i].size/2;
                BoundingBox b = new BoundingBox(rockets[i].position - radius, rockets[i].position + radius);
                Agent a = core.physicsEngine.findAgentIntersection(b, rockets[i].p);
                if (a != null)
                    a.health -= (int)rockets[i].damage;
                if (a != null || rockets[i].collisionDist <= 0) {
                    Weapon.makeExplosion(rockets[i].p, rockets[i].position, rockets[i].explosionSize * 4, rockets[i].explosionSize, rockets[i].damage, rockets[i].explosionTexture);
                    rockets.RemoveAt(i--);
                }
            }

            for (int i = 0; i < explosions.Count; ++i) {
                explosions[i].update2(gameTime);
                if (explosions[i].collisionDist <= 0)
                    explosions.RemoveAt(i--);
            }

            for (int i = lasers.Count - 1; i >= 0; --i) {
                Laser curL = lasers[i];
                curL.update();
                lasers[i] = curL;
                if (lasers[i].timeLeft <= 0)
                    lasers.Remove(lasers[i]);
            }

            for (int i = 0; i < projectiles.Count; ++i) {
                projectiles[i].update(gameTime);
                Vector3 radius = new Vector3(1, 1, 1) * projectiles[i].size / 2;
                BoundingBox b = new BoundingBox(projectiles[i].position - radius, projectiles[i].position + radius);
                Agent a = core.physicsEngine.findAgentIntersection(b, projectiles[i].p);
                if (a != null)
                    a.health -= (int)projectiles[i].damage;
                if (a != null || projectiles[i].collisionDist <= 0) {
                    Weapon.makeExplosion(projectiles[i].p, projectiles[i].position, projectiles[i].explosionSize*2, projectiles[i].explosionSize, projectiles[i].damage , projectiles[i].explosionTexture);
                    projectiles.RemoveAt(i--);
                }
            }
        }
    }

}