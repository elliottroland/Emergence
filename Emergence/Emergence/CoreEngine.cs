using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
using Emergence.Render;
using Emergence.Pickup;
using Emergence.AI;
using Emergence.Weapons;

namespace Emergence
{

    public struct SelectedStruct
    {
        public List<MenuItem> repositionItems;// = new List<MenuItem>();
        public int selectedIndex;// = 0;

    }
    public enum Actions { Unbound = 0, Jump, Reload, Downgrade, Pause, Sprint, Scoreboard, Aim, Fire };
    public enum MenuAction { Unbound = 0, Up, Down, Left, Right, Select, Back, Join, EditConfig };

    public enum GameState { MenuScreen, GameScreen };
    public enum MenuState
    {
        MainMenu,
        SinglePlayer, SplitScreen, Options, Exit,
        StartGame, SelectMap, BotOptions,
        ControlsEdit, PauseMenu, Null
    }

    public class CoreEngine : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;

        public SpriteFont debugFont;
        public Model debugSphere;
        public Model medicross;
        public Model ammoUp;
        public Model arrow;

        public Model cogModel;
        public Texture2D cogTexture;
        public Texture2D bulletTex;
        public Texture2D beamTex;

        public Effect lighting;
        Dictionary<String, Texture2D> textures;
        public Dictionary<String, Texture2D> weaponTrailTextures;

        public MapEngine mapEngine;
        public RenderEngine renderEngine;
        public InputEngine inputEngine;
        public PhysicsEngine physicsEngine;
        public MenuEngine menuEngine;
        public PickUpEngine pickupEngine;
        public AIEngine aiEngine;

        public bool clip = true;

        //test item generator locations
        PickUpGen[] tests = new PickUpGen[4];

        public Player[] players;
        public GameState currentState = GameState.MenuScreen;

        public Dictionary<Type, Texture2D> weaponIcons;

        public CoreEngine()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 576;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //renderEngine = new RenderEngine(this, RenderEngine.Layout.ONE);
            inputEngine = new InputEngine(this);
            physicsEngine = new PhysicsEngine(this);
            aiEngine = new AIEngine(this);

            pickupEngine = new PickUpEngine(this);

            tests[0] = new PickUpGen(this, new Vector3(20, -320, 20), PickUp.PickUpType.AMMO);
            tests[1] = new PickUpGen(this, new Vector3(20, -320, -20), PickUp.PickUpType.HEALTH);
            tests[2] = new PickUpGen(this, new Vector3(-20, -320, 20), PickUp.PickUpType.LEFT);
            tests[3] = new PickUpGen(this, new Vector3(-20, -320, -20), PickUp.PickUpType.RIGHT);

            players = new Player[0];
            /*players = new Player[3];
            players[0] = new Player(this, PlayerIndex.One, Vector3.Zero);
            players[1] = new Player(this, PlayerIndex.Two, Vector3.Zero);
            players[2] = new Player(this, PlayerIndex.Three, new Vector3(100, 0, 0));*/

            //call the super
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);


            //load the trextures
            textures = new Dictionary<string, Texture2D>();
            /*textures.Add("base_floor/pool_side2", Content.Load<Texture2D>("textures/base_floor/pool_side2"));
            textures.Add("base_wall/atech1_f", Content.Load<Texture2D>("textures/base_wall/atech1_f"));
            textures.Add("sfx/xmetalfloor_wall_5b", Content.Load<Texture2D>("textures/sfx/xmetalfloor_wall_5b"));
            textures.Add("ctf/metal_b", Content.Load<Texture2D>("textures/ctf/metal_b"));
            textures.Add("ctf/clangdark", Content.Load<Texture2D>("textures/ctf/clangdark"));
            textures.Add("ctf/pittedrust3stripes_fix", Content.Load<Texture2D>("textures/ctf/pittedrust3stripes_fix"));
            textures.Add("common/caulk", Content.Load<Texture2D>("textures/common/caulk"));
            textures.Add("emergence/floor_hex", Content.Load < Texture2D("textures/emgergence/floor_hex"));*/
            using (System.IO.StreamReader sr = System.IO.File.OpenText("Content/textures/texturelist.txt"))
            {
                string tex = "";
                while ((tex = sr.ReadLine()) != null)
                {
                    tex = tex.Trim();
                    if (tex.Length > 0 && !tex.StartsWith("//"))
                        textures.Add(tex, Content.Load<Texture2D>("textures/" + tex));
                }
            }

            //load trails for weapons
            weaponTrailTextures = new Dictionary<string, Texture2D>();
            using (System.IO.StreamReader sr = System.IO.File.OpenText("Content/weaponTrails/weapontraillist.txt")) {
                string tex = "";
                while ((tex = sr.ReadLine()) != null) {
                    tex = tex.Trim();
                    string[] split = Regex.Split(tex, @"\s+");
                    weaponTrailTextures.Add(split[0], Content.Load<Texture2D>("weaponTrails/" + split[1]));
                }
            }

            //load menu content
            cogModel = Content.Load<Model>("CogAttempt");
            cogTexture = Content.Load<Texture2D>("line");

            //load shader
            lighting = Content.Load<Effect>("PointLighting");


            menuEngine = new MenuEngine(this);


            debugFont = Content.Load<SpriteFont>("debugFont");
            debugSphere = Content.Load<Model>("SphereHighPoly");
            medicross = Content.Load<Model>("Medicross");
            ammoUp = Content.Load<Model>("bullets");
            arrow = Content.Load<Model>("arrow");

            //loadMap("Content/maps/test2.map");
            //startGame("test2", new bool[] { true, false, true, false });


        }

        public void loadMap(String mapFile)
        {
            mapEngine = new MapEngine(this, mapFile, textures);
            aiEngine.generateAIMesh();
            physicsEngine.generateCollisionGrid(500);
        }

        public void startGame(string mapName, bool[] playersPlaying)
        {
            loadMap("Content/maps/" + mapName + ".map");


            List<PlayerIndex> playerIndicesPlaying = new List<PlayerIndex>();
            for (int i = 0; i < playersPlaying.Length; ++i)
                if (playersPlaying[i])
                    playerIndicesPlaying.Add(PlayerIndex.One + i);
            players = new Player[playerIndicesPlaying.Count];
            PlayerIndex[] playerIndices = new PlayerIndex[playerIndicesPlaying.Count];
            for (int i = 0; i < players.Length; ++i)
            {
                players[i] = new Player(this, playerIndicesPlaying[i], mapEngine.playerPoss[aiEngine.random.Next(mapEngine.playerPoss.Count)]);
                playerIndices[i] = playerIndicesPlaying[i];
            }
            renderEngine = new RenderEngine(this, playerIndices);

            //once the map is loaded the player has a position, so take it for the test agent
            for (int i = 0; i < 1; ++i) aiEngine.agents.Add(new AIAgent(this, players[0].position));

            currentState = GameState.GameScreen;
        }

        public void spawnPlayer(Agent p) {
            //find the spawn point furthest from other agents
            float maxDist = -float.MaxValue;
            Vector3 minPoint = Vector3.Zero;
            foreach (Vector3 pos in mapEngine.playerPoss) {
                float dist = 0;
                foreach (Agent a in allAgents())
                    dist += Vector3.Distance(a.position, pos);
                if (dist > maxDist) {
                    maxDist = dist;
                    minPoint = pos;
                }
            }

            p.position = minPoint;
        }

        public Player getPlayerForIndex(PlayerIndex pi)
        {
            for (int i = 0; i < players.Length; ++i)
                if (players[i].playerIndex == pi)
                    return players[i];
            return null;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        public IEnumerable<ICollidable> allCollidables()
        {
            foreach (Brush b in mapEngine.brushes)
                yield return b;
            foreach(Agent a in allAgents())
                yield return a;
        }

        public IEnumerable<Agent> allAgents() {
            foreach (Agent a in players)
                yield return a;
            foreach(Agent a in aiEngine.agents)
                yield return a;
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Home) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Back))
                this.Exit();

            inputEngine.Update();

            if (currentState == GameState.MenuScreen)
                menuEngine.Update(gameTime);
            else
            {

                pickupEngine.Update(gameTime);
                foreach (Player player in players)
                    player.Update(gameTime);

                aiEngine.Update(gameTime);
                renderEngine.Update(gameTime);
            }

            base.Update(gameTime);
        }

        public void printList(VertexPositionNormalTexture[] pts)
        {
            foreach (VertexPositionNormalTexture p in pts)
                Console.WriteLine(p.Position);
            Console.WriteLine("\n");
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.DarkGray);
            if (currentState == GameState.GameScreen)
            {
                foreach (Player p in players)
                    renderEngine.updateCameraForPlayer(p.playerIndex);
                renderEngine.Draw(gameTime);
            }
            if (currentState == GameState.MenuScreen)
            {
                menuEngine.Draw(gameTime);
            }

            //DrawStringDebug("" + 1000 / gameTime.ElapsedGameTime.Milliseconds);
            if(players.Length > 0)
                DrawStringDebug("health: " + players[0].health
                                    + "\nammo: " + players[0].ammo
                                    + "\nweapon: " + players[0].equipped.GetType().Name);

            base.Draw(gameTime);
        }

        //Debugging methods

        public Texture2D GenTextureDebug(String text)
        {

            int w = (int)debugFont.MeasureString(text).X;
            int h = (int)debugFont.MeasureString(text).Y;

            RenderTarget2D render = new RenderTarget2D(GraphicsDevice, w, h, 1, GraphicsDevice.DisplayMode.Format
                                    , GraphicsDevice.PresentationParameters.MultiSampleType
                                    , GraphicsDevice.PresentationParameters.MultiSampleQuality
                                    , RenderTargetUsage.DiscardContents);

            GraphicsDevice.SetRenderTarget(0, render);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            spriteBatch.DrawString(debugFont, text, new Vector2(1, 1), Color.Black);
            spriteBatch.DrawString(debugFont, text, new Vector2(0, 0), Color.White);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(0, null);
            return render.GetTexture();

        }

        public void DrawTextureDebug(Texture2D text)
        {

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            spriteBatch.Draw(text, new Rectangle(64, 64, text.Width, text.Height), Color.White);
            spriteBatch.End();

        }

        public void DrawStringDebug(String text)
        {

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            spriteBatch.DrawString(debugFont, text, new Vector2(65, 65), Color.Black);
            spriteBatch.DrawString(debugFont, text, new Vector2(64, 64), Color.White);
            spriteBatch.End();

        }

    }
}
