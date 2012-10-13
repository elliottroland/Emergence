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
using Nuclex.Fonts;
using Nuclex.Graphics;


namespace Emergence
{
    //----------------------------------------------------------------------------------
    public class TitleScreen : MenuScreen
    {
        CoreEngine coreEngine;
        public Texture2D background, title, light;

        Vector2 titlePos = new Vector2(100, 100);
        float titleScale = 0.5f;
        float titleAlpha = 0.0f;

        float alphaInc = 0.01f;



        float totalMilli = 0;

        public String start = "Press Start";
        float startAlpha = 0.0f;
        bool showStart = false;

        public TitleScreen(MenuEngine m, CoreEngine g)
        {
            coreEngine = g;
            menuEngine = m;
            type = "Title";
            LoadContent();
        }

        public override void LoadContent()
        {
            title = coreEngine.Content.Load<Texture2D>("MenuTextures/EmergenceIntro2");
            background = coreEngine.Content.Load<Texture2D>("MenuTextures/background2");
            light = coreEngine.Content.Load<Texture2D>("MenuTextures/menuLight");
        }


        public override MenuScreen Update(GameTime time)
        {
            totalMilli += time.ElapsedGameTime.Milliseconds;
            if (totalMilli > 1000)
                showStart = true;

            if (startAlpha > 1 || startAlpha < 0)
                alphaInc *= -1;

            startAlpha += alphaInc;

            if (titleScale < 1)
                titleScale += 0.0013f;

            if (titleAlpha < 1)
                titleAlpha += 0.004f;


            menuActions = coreEngine.inputEngine.getMenuKeys();
            List<MenuAction> menuActionsController1 = coreEngine.inputEngine.getMenuButtons(PlayerIndex.One);


            foreach (MenuAction m in menuActionsController1)
                menuActions.Add(m);

            if (menuActions.Contains(MenuAction.Join))
                return menuEngine.mainMenu;

            return this;


        }


        public override void Draw()
        {
            Console.WriteLine(background.Width);
            coreEngine.spriteBatch.Begin();
            coreEngine.spriteBatch.Draw(background, new Rectangle(0, 0, background.Width, background.Height), Color.White);
           // coreEngine.spriteBatch.Draw(light, new Rectangle(0, 0, light.Width, light.Height), Color.White);
            coreEngine.spriteBatch.Draw(title, screenCenter, new Rectangle(0, 0, title.Width, title.Height), new Color(new Vector4(Color.White.ToVector3(), titleAlpha)), 0f, new Vector2(title.Width / 2, title.Height / 2), titleScale, SpriteEffects.None, 0);

            if (showStart)
                coreEngine.spriteBatch.DrawString(coreEngine.debugFont, start, new Vector2(screenCenter.X - (coreEngine.debugFont.MeasureString(start).X / 2), screenCenter.Y + 170), new Color(new Vector4(Color.White.ToVector3(), startAlpha)));

            coreEngine.spriteBatch.End();



        }


    }
    //----------------------------------------------------------------------------------
    public class MainMenuScreen : MenuScreen
    {

        public Texture2D background, light, selectWheel;
       



        public MainMenuScreen(MenuEngine m, CoreEngine g)
        {
            coreEngine = g;
            menuEngine = m;
            menuItems.Add(new MenuItem("Single Player", MenuState.SinglePlayer));
            menuItems.Add(new MenuItem("Split Screen", MenuState.SinglePlayer));
            menuItems.Add(new MenuItem("Options", MenuState.SinglePlayer));
            menuItems.Add(new MenuItem("Exit", MenuState.Exit));
            LoadContent();
        }

       

        public override void LoadContent()
        {
            background = coreEngine.Content.Load<Texture2D>("MenuTextures/background2");
            light = coreEngine.Content.Load<Texture2D>("MenuTextures/menuLight");
            selectWheel = coreEngine.Content.Load<Texture2D>("MenuTextures/SelectionWheel2");
        }



        public override MenuScreen Update(GameTime g)
        {
            getInput();
            Console.WriteLine("SelectIndex: "+selectIndex);
            MenuItem temp = menuItems.ElementAt<MenuItem>(selectIndex);
            if (menuActions.Contains<MenuAction>(MenuAction.Select))
            {
                if (temp.label == "Single Player")
                {
                    coreEngine.currentState = GameState.GameScreen;
                    return this;
                }
                if (temp.label == "Split Screen") { return menuEngine.splitScreenMenu; }
                if (temp.label == "Options") { return menuEngine.optionsMenu; }
                if (temp.label == "Exit") { coreEngine.Exit(); }
            }
            

            return this;


        }


        public override void Draw()
        {
            coreEngine.spriteBatch.Begin();
            coreEngine.spriteBatch.Draw(background, new Rectangle(0, 0, background.Width, background.Height), Color.White);
          
            //coreEngine.spriteBatch.Draw(light, new Rectangle(0, 0, light.Width, light.Height), Color.White);
            coreEngine.spriteBatch.Draw(selectWheel, selectWheelPos, new Rectangle(0, 0, selectWheel.Width, selectWheel.Height), Color.White, selectWheelRot, new Vector2(selectWheel.Width / 2, selectWheel.Height / 2 + 25), 0.9f, SpriteEffects.None, 0);

            foreach (MenuItem m in menuItems)
                if (!m.selected)
                    coreEngine.spriteBatch.DrawString(coreEngine.debugFont, m.label, m.position + new Vector2(2, 2), Color.White);
                else
                    coreEngine.spriteBatch.DrawString(coreEngine.debugFont, m.label, m.position, Color.Red);

            coreEngine.spriteBatch.End();

        }

    }
    /*
    //----------------------------------------------------------------------------------
    public class SinglePlayerScreen : MenuScreen
    {
        CoreEngine coreEngine;
        public Texture2D background, light, selectWheel;
        float selectWheelRot = 0;
        List<MenuItem> menuItems = new List<MenuItem>();


        public SinglePlayerScreen(MenuEngine m, CoreEngine g)
        {
            menuEngine = menuEngine;
            coreEngine = g;
        }

        public override void LoadContent()
        {
            background = coreEngine.Content.Load<Texture2D>("MenuTextures/background");
            light = coreEngine.Content.Load<Texture2D>("MenuTextures/menuLight");
            selectWheel = coreEngine.Content.Load<Texture2D>("MenuTextures/SelectionWheel");
        }


        public override void Update(GameTime g)
        {


        }


        public override void Draw()
        {


        }

    }

    //----------------------------------------------------------------------------------
    public class BotNumberMenuScreen : MenuScreen
    {
        CoreEngine coreEngine;
        public Texture2D background, light, selectWheel;
        float selectWheelRot = 0;



        public BotNumberMenuScreen(MenuEngine m, CoreEngine g)
        {
            menuEngine = menuEngine;
            coreEngine = g;
        }
        public override void LoadContent()
        {

        }


        public override void Update(GameTime g)
        {


        }


        public override void Draw()
        {


        }

    }

    //----------------------------------------------------------------------------------
    public class SplitScreen : MenuScreen
    {
        CoreEngine coreEngine;
        public Texture2D background, light, selectWheel;
        float selectWheelRot = 0;




        public SplitScreen(MenuEngine m, CoreEngine g)
        {
            menuEngine = menuEngine;
            coreEngine = g;
        }
        public override void LoadContent()
        {

        }


        public override void Update(GameTime g)
        {


        }


        public override void Draw()
        {


        }

    }

    public class OptionsMenuScreen : MenuScreen
    {
        CoreEngine coreEngine;
        public Texture2D background, light, selectWheel;
        float selectWheelRot = 0;



        public OptionsMenuScreen(MenuEngine m, CoreEngine g)
        {
            menuEngine = menuEngine;
            coreEngine = g;
        }
        public override void LoadContent()
        {

        }


        public override void Update(GameTime g)
        {


        }


        public override void Draw()
        {


        }

    }


    */


}