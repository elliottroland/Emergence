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



namespace Emergence
{


    public class MenuEngine
    {
        public MenuScreen currentMenu;
        private CoreEngine coreEngine;
        public MenuScreen mainMenu, singlePlayerMenu, botNumberMenu, splitScreenMenu, optionsMenu, pauseMenu,loadScreen;
        TitleScreen titleMenu;

        List<MenuItem[]> menuStack = new List<MenuItem[]>();
        public float selectAngle;

        bool setOverlayMode = false;

        int maxPlayersInMap = 8;
        int selectedNumBots = 0;

        public MenuEngine(CoreEngine g)
        {
            coreEngine = g;
            MenuScreen.loadButtonTextures(g);

            titleMenu = new TitleScreen(this, g);
            mainMenu = new MainMenuScreen(this, g);            
            singlePlayerMenu = new SinglePlayerScreen(this, g);            

            botNumberMenu = new BotNumberMenuScreen(this, g);            
            splitScreenMenu = new SplitScreen(this, g);
            pauseMenu = new PauseMenu(this, g);
            loadScreen = new LoadScreen(this, g);

            /*
            optionsMenu = new OptionsMenuScreen(this, g);
             */



            currentMenu = titleMenu;


        }

        public void LoadContent()
        {


        }


        public void Update(GameTime gameTime)
        {
            currentMenu = currentMenu.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            currentMenu.Draw();
        }
    }


}