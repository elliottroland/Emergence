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
    public class LoadScreen : MenuScreen
    {
        float wheelRot = 0;
        public LoadScreen(MenuEngine m, CoreEngine g)
        {
            coreEngine = g;
            menuEngine = m;
            type = "Title";

        }


        public override MenuScreen Update(GameTime time)
        {
            Console.WriteLine("Updating loadscreen");
            return this;
        }
        public override void Draw()
        {
            //Draw Backgrounds           
            //Console.WriteLine(background.Width);
            coreEngine.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            coreEngine.spriteBatch.Draw(background, new Rectangle(0, 0, background.Width, background.Height), Color.White);
           
            coreEngine.spriteBatch.Draw(selectWheel, selectWheelPos, new Rectangle(0, 0, selectWheel.Width, selectWheel.Height),
                Color.White, wheelRot+=0.1f, new Vector2(selectWheel.Width / 2, selectWheel.Height / 2 + 25),
                0.85f, SpriteEffects.None, 0);

         
            coreEngine.spriteBatch.End();



        }
    }




    //----------------------------------------------------------------------------------
    public class TitleScreen : MenuScreen
    {



        Vector2 titlePos = new Vector2(100, 100);
        float titleScale = 0.5f;
        float titleAlpha = 0.0f;

        float alphaInc = 0.004f;



        float totalMilli = 0;

        public String start = "Press Start";
        float startAlpha = 0.0f;
        bool showStart = false;

        public TitleScreen(MenuEngine m, CoreEngine g)
        {
            coreEngine = g;
            menuEngine = m;
            type = "Title";

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
            //Draw Backgrounds           

            //Console.WriteLine(background.Width);

            coreEngine.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            coreEngine.spriteBatch.Draw(background, new Rectangle(0, 0, background.Width, background.Height), Color.White);
            coreEngine.spriteBatch.Draw(title, screenCenter, new Rectangle(0, 0, title.Width, title.Height), new Color(new Vector4(Color.White.ToVector3(), titleAlpha)), 0f, new Vector2(title.Width / 2, title.Height / 2), titleScale, SpriteEffects.None, 0);

            if (showStart)
                coreEngine.spriteBatch.DrawString(coreEngine.debugFont, start, new Vector2(screenCenter.X - (coreEngine.debugFont.MeasureString(start).X / 2), screenCenter.Y + 170), new Color(new Vector4(Color.White.ToVector3(), startAlpha)));

            coreEngine.spriteBatch.End();



        }


    }
    //----------------------------------------------------------------------------------
    public class MainMenuScreen : MenuScreen
    {

        public MainMenuScreen(MenuEngine m, CoreEngine g)
        {
            coreEngine = g;
            menuEngine = m;
            menuItems.Add(new MenuItem("Main Menu", MenuState.Null));
            menuItems.Add(new MenuItem("Single Player", MenuState.SinglePlayer));
            menuItems.Add(new MenuItem("Split Screen", MenuState.SplitScreen));
            //menuItems.Add(new MenuItem("Options", MenuState.Options));
            menuItems.Add(new MenuItem("Exit", MenuState.Exit));

        }



        public override MenuScreen Update(GameTime g)
        {
            getInput();

            //Console.WriteLine("SelectIndex: " + selectIndex);


            if (menuActions.Contains<MenuAction>(MenuAction.Select))
            {
                if (selectIndex == 0 || selectIndex == 1)
                {
                    selectIndex = 1;
                    dist = 0;
                }
                MenuItem temp = menuItems.ElementAt<MenuItem>(selectIndex);
                if (temp.label == "Single Player") { return menuEngine.singlePlayerMenu; }
                if (temp.label == "Split Screen") { return menuEngine.splitScreenMenu; }
                //if (temp.label == "Options") { return menuEngine.optionsMenu; }
                if (temp.label == "Exit") { coreEngine.Exit(); }
            }


            return this;


        }


        public override void Draw()
        {
            //Draw Backgrounds
            coreEngine.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            coreEngine.spriteBatch.Draw(background, new Rectangle(0, 0, background.Width, background.Height), Color.White);

            coreEngine.spriteBatch.Draw(selectWheel, selectWheelPos, new Rectangle(0, 0, selectWheel.Width, selectWheel.Height),
                Color.White, selectWheelRot, new Vector2(selectWheel.Width / 2, selectWheel.Height / 2 + 25),
                0.85f, SpriteEffects.None, 0);

            drawTips();

            //Draw Menu Items
            foreach (MenuItem m in menuItems)
                if (!m.selected)
                    coreEngine.spriteBatch.DrawString(coreEngine.debugFont, m.label, m.position + new Vector2(2, 2), Color.White);
                else
                    coreEngine.spriteBatch.DrawString(coreEngine.debugFont, m.label, m.position, Color.Red);

            coreEngine.spriteBatch.End();

        }

    }

    //----------------------------------------------------------------------------------
    public class SinglePlayerScreen : MenuScreen
    {

        public SinglePlayerScreen(MenuEngine m, CoreEngine g)
        {
            coreEngine = g;
            menuEngine = m;
            menuItems.Add(new MenuItem("Single Player Menu", MenuState.Null));
            menuItems.Add(new MenuItem("Start Playing", MenuState.StartGame));
            menuItems.Add(new MenuItem("Number of Bots: ", MenuState.BotOptions));
            menuItems.Add(new MenuItem("Map Select: ", MenuState.SelectMap));
            //menuItems.Add(new MenuItem("Exit", MenuState.Exit));

        }





        public override MenuScreen Update(GameTime g)
        {
            getInput();

            if (menuActions.Contains<MenuAction>(MenuAction.Select))
            {

                dist = 0;

                MenuItem temp = menuItems.ElementAt<MenuItem>(selectIndex);
                if (temp.nextMenu == MenuState.StartGame)
                {
                                   


                    coreEngine.startGame("test3", new bool[]{true, false,false,false});


                    return this;
                }
                if (temp.nextMenu == MenuState.BotOptions) { return menuEngine.botNumberMenu; }
                if (temp.nextMenu == MenuState.SelectMap) { return menuEngine.optionsMenu; }
                // if (temp.label == "Exit") { coreEngine.Exit(); }
            }
            if (menuActions.Contains<MenuAction>(MenuAction.Back))
            {
                dist = 2 * maxDist;
                return menuEngine.mainMenu;
            }


            return this;


        }


        public override void Draw()
        {
            //Draw Backgrounds            
            coreEngine.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            coreEngine.spriteBatch.Draw(background, new Rectangle(0, 0, background.Width, background.Height), Color.White);

            coreEngine.spriteBatch.Draw(selectWheel, selectWheelPos, new Rectangle(0, 0, selectWheel.Width, selectWheel.Height),
                Color.White, selectWheelRot, new Vector2(selectWheel.Width / 2, selectWheel.Height / 2 + 25),
                0.85f, SpriteEffects.None, 0);

            drawTips();

            //Draw Menu Items
            foreach (MenuItem m in menuItems)
                if (!m.selected)
                    coreEngine.spriteBatch.DrawString(coreEngine.debugFont, m.label, m.position + new Vector2(2, 2), Color.White);
                else
                    coreEngine.spriteBatch.DrawString(coreEngine.debugFont, m.label, m.position, Color.Red);

            coreEngine.spriteBatch.End();

        }

    }

    //----------------------------------------------------------------------------------
    public class BotNumberMenuScreen : MenuScreen
    {


        public BotNumberMenuScreen(MenuEngine m, CoreEngine g)
        {
            menuEngine = m;
            coreEngine = g;
            menuItems.Add(new MenuItem("Bot Options", MenuState.Null));
            menuItems.Add(new MenuItem("1", MenuState.Null));
            menuItems.Add(new MenuItem("2", MenuState.Null));
            menuItems.Add(new MenuItem("3", MenuState.Null));
            menuItems.Add(new MenuItem("4", MenuState.Null));
            menuItems.Add(new MenuItem("5", MenuState.Null));
            menuItems.Add(new MenuItem("6", MenuState.Null));
            menuItems.Add(new MenuItem("7", MenuState.Null));


        }


        public override MenuScreen Update(GameTime g)
        {

            getInput();

            if (menuActions.Contains<MenuAction>(MenuAction.Select))
            {

                dist = 0;

                MenuItem temp = menuItems.ElementAt<MenuItem>(selectIndex);

                //save number of bots
                //coreEngine.numberOfBots = ?;
                return menuEngine.singlePlayerMenu;

            }
            if (menuActions.Contains<MenuAction>(MenuAction.Back))
            {
                dist = 2 * maxDist;
                return menuEngine.singlePlayerMenu;
            }



            return this;
        }


        public override void Draw()
        {
            //Draw Backgrounds
            coreEngine.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            coreEngine.spriteBatch.Draw(background, new Rectangle(0, 0, background.Width, background.Height), Color.White);
            coreEngine.spriteBatch.Draw(selectWheel, selectWheelPos, new Rectangle(0, 0, selectWheel.Width, selectWheel.Height),
                Color.White, selectWheelRot, new Vector2(selectWheel.Width / 2, selectWheel.Height / 2 + 25),
                0.85f, SpriteEffects.None, 0);

            drawTips();

            //Draw Menu Items
            foreach (MenuItem m in menuItems)
                if (!m.selected)
                    coreEngine.spriteBatch.DrawString(coreEngine.debugFont, m.label, m.position + new Vector2(2, 2), Color.White);
                else
                    coreEngine.spriteBatch.DrawString(coreEngine.debugFont, m.label, m.position, Color.Red);

            coreEngine.spriteBatch.End();

        }

    }

    //----------------------------------------------------------------------------------
    public class SplitScreen : MenuScreen
    {
        bool[] playerJoined = new bool[4];
        bool notEnoughPlayers = false;

        long initialTime = 0, currentTime = 0;


        public SplitScreen(MenuEngine m, CoreEngine g)
        {
            menuEngine = m;
            coreEngine = g;

            menuItems.Add(new MenuItem("Split Screen", MenuState.Null));

        }


        public override MenuScreen Update(GameTime g)
        {

            bool wantToQuit = false;
            //check keyboard join
            //keyboard is player one
            if (coreEngine.inputEngine.getMenuKeys().Contains(MenuAction.Select))
            {
                initialTime = currentTime;
                playerJoined[0] = true;
            }
            if (coreEngine.inputEngine.getMenuKeys().Contains(MenuAction.Back))
            {
                if (playerJoined[0])
                    playerJoined[0] = false;
                else
                    wantToQuit = true;
            }

            //Check for controller joins
            for (int i = 0; i < 4; i++)
            {
                if (coreEngine.inputEngine.getMenuButtons((PlayerIndex)i).Contains(MenuAction.Select))
                    playerJoined[i] = true;

                if (coreEngine.inputEngine.getMenuButtons((PlayerIndex)i).Contains(MenuAction.Back))
                {
                    if (playerJoined[i])
                        playerJoined[i] = false;
                    else
                        wantToQuit = true;
                }
                if (coreEngine.inputEngine.getMenuButtons((PlayerIndex)i).Contains(MenuAction.Join))
                {
                    if (getJoinedCount() < 2)
                    {
                        initialTime = currentTime;
                        notEnoughPlayers = true;
                    }
                    else
                        coreEngine.startGame("test2", playerJoined);
                }
            }

            if (wantToQuit && getJoinedCount() == 0)
            {
                dist = 2 * maxDist;
                return menuEngine.mainMenu;
            }

            return this;

        }

        public int getJoinedCount()
        {
            int count = 0;
            for (int i = 0; i < 4; i++)
                if (playerJoined[i])
                    count++;

            return count;
        }


        public override void Draw()
        {
            //Draw Backgrounds
            coreEngine.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            coreEngine.spriteBatch.Draw(background, new Rectangle(0, 0, background.Width, background.Height), Color.White);
            //coreEngine.spriteBatch.Draw(selectWheel, selectWheelPos, new Rectangle(0, 0, selectWheel.Width, selectWheel.Height),
            //  Color.White, selectWheelRot, new Vector2(selectWheel.Width / 2, selectWheel.Height / 2 + 25),
            //0.85f, SpriteEffects.None, 0);           


            Vector2[] pos = new Vector2[4];
            Vector2 space = new Vector2(110, 0);
            pos[0] = screenCenter + new Vector2(-screenWidth / 4, -screenHeight / 4) - space;
            pos[1] = screenCenter + new Vector2(screenWidth / 4, -screenHeight / 4) - space;
            pos[2] = screenCenter + new Vector2(screenWidth / 4, screenHeight / 4) - space;
            pos[3] = screenCenter + new Vector2(-screenWidth / 4, screenHeight / 4) - space;

            coreEngine.spriteBatch.DrawString(coreEngine.debugFont, "Split Screen", new Vector2(screenCenter.X - (coreEngine.debugFont.MeasureString("Split Screen") / 2).X, 50), Color.White);
            /*
            coreEngine.spriteBatch.Draw(line, screenCenter, new Rectangle(0,0, line.Width, line.Height),
                       Color.White, 0f, new Vector2(line.Width / 2, line.Height / 2),
                       1.6f, SpriteEffects.None, 0);

            coreEngine.spriteBatch.Draw(line, screenCenter+new Vector2(0,200), new Rectangle(0,0, line.Width, line.Height),
                       Color.White, MathHelper.PiOver2, new Vector2(line.Width / 2, line.Height / 2),
                       1.6f, SpriteEffects.None, 0);

            */
            coreEngine.spriteBatch.Draw(loadWheel, screenCenter, new Rectangle(0, 0, loadWheel.Width, loadWheel.Height),
                        Color.White, 0f, new Vector2(loadWheel.Width / 2, loadWheel.Height / 2 + 25),
                        0.2f, SpriteEffects.None, 0);


            String output = "";
            for (int i = 0; i < 4; i++)
                if (playerJoined[i])
                {
                    coreEngine.spriteBatch.Draw(splitWheel, screenCenter - new Vector2(3, -2), new Rectangle(0, 0, splitWheel.Width, splitWheel.Height),
                        Color.White, MathHelper.PiOver2 * i - MathHelper.PiOver4, new Vector2(splitWheel.Width / 2, splitWheel.Height / 2 + 25),
                        0.2f, SpriteEffects.None, 0);
                    coreEngine.spriteBatch.DrawString(coreEngine.debugFont, "Player " + (i + 1) + " has joined", pos[i], Color.White);
                }
                else
                {
                    coreEngine.spriteBatch.DrawString(coreEngine.debugFont, "Press ", pos[i], Color.White);
                    coreEngine.spriteBatch.Draw(A_button, pos[i] + new Vector2(coreEngine.debugFont.MeasureString("Press  ").X, -10), null, Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 1f);
                    coreEngine.spriteBatch.DrawString(coreEngine.debugFont, "to join ", pos[i] + new Vector2(150, 0), Color.White);
                }



            if (notEnoughPlayers)
                output += "\nThere are not enough players to start\nAt least 2 required";


            coreEngine.spriteBatch.End();
            coreEngine.DrawStringDebug(output);

        }

    }



    public class PauseMenu : MenuScreen
    {
     
        
        public PauseMenu(MenuEngine m, CoreEngine g)
        {
            menuEngine = m;
            coreEngine = g;

            menuItems.Add(new MenuItem("Pause Menu", MenuState.Null));
            menuItems.Add(new MenuItem("Resume Game", MenuState.ResumeGame));
            menuItems.Add(new MenuItem("Main Menu", MenuState.MainMenu));        


        }


        public override MenuScreen Update(GameTime g)
        {
            getInput(true);


            if (menuActions.Contains<MenuAction>(MenuAction.Select))
            {
                dist = 0;

                MenuItem temp = menuItems.ElementAt<MenuItem>(selectIndex);

                if (temp.nextMenu == MenuState.ResumeGame)
                    coreEngine.currentState = GameState.GameScreen;

                if (temp.nextMenu == MenuState.MainMenu)
                    return menuEngine.mainMenu;
                //save number of bots
                //coreEngine.numberOfBots = ?;
                //return menuEngine.singlePlayerMenu;

            }         



            return this;
        }


        public override void Draw()
        {
            //Draw Backgrounds
            coreEngine.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            coreEngine.spriteBatch.Draw(background, new Rectangle(0, 0, background.Width, background.Height), Color.White);
            coreEngine.spriteBatch.Draw(selectWheel, selectWheelPos, new Rectangle(0, 0, selectWheel.Width, selectWheel.Height),
                Color.White, selectWheelRot, new Vector2(selectWheel.Width / 2, selectWheel.Height / 2 + 25),
                0.85f, SpriteEffects.None, 0);

            drawTips();

            //Draw Menu Items
            foreach (MenuItem m in menuItems)
                if (!m.selected)
                    coreEngine.spriteBatch.DrawString(coreEngine.debugFont, m.label, m.position + new Vector2(2, 2), Color.White);
                else
                    coreEngine.spriteBatch.DrawString(coreEngine.debugFont, m.label, m.position, Color.Red);

            coreEngine.spriteBatch.End();

        }


    }





}