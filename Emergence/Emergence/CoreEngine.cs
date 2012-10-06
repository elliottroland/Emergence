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

using Nuclex.Fonts;
using Nuclex.Graphics;

namespace Emergence {
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

    public class CoreEngine : Microsoft.Xna.Framework.Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont debugFont;
        public Model debugSphere;

        public Model cogModel;
        public Texture2D cogTexture;
        public Texture2D bulletTex;

        public Effect lighting;


        public MapEngine mapEngine;
        public RenderEngine renderEngine;
        public InputEngine inputEngine;
        public PhysicsEngine physicsEngine;
        public MenuEngine menuEngine;
        public PickUpEngine pickupEngine;

        //test item generator locations
        PickUpGen[] tests = { new PickUpGen(new Vector3(20, -320, 20), PickUp.PickUpType.AMMO), new PickUpGen(new Vector3(20, -320, -20), PickUp.PickUpType.HEALTH)
                            ,new PickUpGen(new Vector3(-20, -320, 20), PickUp.PickUpType.LEFT), new PickUpGen(new Vector3(-20, -320, -20), PickUp.PickUpType.RIGHT)};

        public Player[] players;
        public GameState currentState = GameState.MenuScreen;

        public CoreEngine() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            renderEngine = new RenderEngine(this, RenderEngine.Layout.ONE);
            inputEngine = new InputEngine(this);
            physicsEngine = new PhysicsEngine(this);

            pickupEngine = new PickUpEngine(tests);

            players = new Player[1];
            players[0] = new Player(this, PlayerIndex.One, Vector3.Zero);
            /*players[1] = new Player(this, PlayerIndex.Two, Vector3.Zero);
            players[2] = new Player(this, PlayerIndex.Three, new Vector3(100, 0, 0));*/

            //call the super
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            

            //load the trextures
            Dictionary<String, Texture2D> textures = new Dictionary<string, Texture2D>();
            textures.Add("base_floor/pool_side2", Content.Load<Texture2D>("textures/base_floor/pool_side2"));
            textures.Add("base_wall/atech1_f", Content.Load<Texture2D>("textures/base_wall/atech1_f"));
            textures.Add("sfx/xmetalfloor_wall_5b", Content.Load<Texture2D>("textures/sfx/xmetalfloor_wall_5b"));
            textures.Add("ctf/metal_b", Content.Load<Texture2D>("textures/ctf/metal_b"));
            textures.Add("ctf/clangdark", Content.Load<Texture2D>("textures/ctf/clangdark"));
            textures.Add("ctf/pittedrust3stripes_fix", Content.Load<Texture2D>("textures/ctf/pittedrust3stripes_fix"));
            textures.Add("common/caulk", Content.Load<Texture2D>("textures/common/caulk"));

            mapEngine = new MapEngine(this, "Content/maps/test.map", textures);

            //load menu content
            cogModel = Content.Load<Model>("CogAttempt");
            cogTexture = Content.Load<Texture2D>("line");
            bulletTex = Content.Load<Texture2D>("bullet");

            //load shader
            lighting = Content.Load<Effect>("Lighting");
            lighting.CurrentTechnique = lighting.Techniques["Technique1"];

            VectorFont vectorFont = Content.Load<VectorFont>("menuFont");           
            menuEngine = new MenuEngine(this, vectorFont);

            debugFont = Content.Load<SpriteFont>("debugFont");
            debugSphere = Content.Load<Model>("SphereHighPoly");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
            
        }

        protected override void Update(GameTime gameTime) {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Home)||GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Back))
                this.Exit();

            inputEngine.Update();

            if (currentState == GameState.MenuScreen)
                menuEngine.Update();
            else
            {

                pickupEngine.Update(gameTime);
                foreach (Player player in players)
                    player.Update(gameTime);
            
            }

            base.Update(gameTime);
        }

        public void printList(VertexPositionNormalTexture[] pts) {
            foreach (VertexPositionNormalTexture p in pts)
                Console.WriteLine(p.Position);
            Console.WriteLine("\n");
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
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

            base.Draw(gameTime);
        }


        public void DrawTextDebug(String text)
        {

            spriteBatch.Begin();
            spriteBatch.DrawString(debugFont, text, new Vector2(65, 65), Color.Black);
            spriteBatch.DrawString(debugFont, text, new Vector2(64, 64), Color.White);
            spriteBatch.End();

        }
    }
}
