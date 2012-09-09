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

    public class MenuEngine
    {
        //MenuStates currentMenu = MenuStates.MainMenu;
        private CoreEngine coreEngine;

        Menu mainMenu, singlePlayerMenu, selectMapMenu, botNumberMenu,splitScreenMenu, optionsMenu;
        public Menu currentMenu;
        List<MenuItem[]> menuStack = new List<MenuItem[]>();     
        public float selectAngle;
       

        bool setOverlayMode = false;
        int maxPlayersInMap = 7;
        int selectedNumBots = 0;

        VectorFont vecFont;

        Matrix view, projection;

        Vector3 camPos = new Vector3(220, -90, 240);
        Vector3 camLook = new Vector3(160, -80, 0);


        TextBatch textBatch;

        public MenuEngine(CoreEngine g, VectorFont font)
        {
            coreEngine = g;
            vecFont = font;
            textBatch = new TextBatch(coreEngine.GraphicsDevice);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2,
                                                                   4/3,
                                                                   1,
                                                                   10000);
            view = Matrix.CreateLookAt(camPos, camLook, Vector3.Up);


            mainMenu = new Menu(MenuState.MainMenu);            
            //mainMenu.offset = new Vector2(150, 50);
            mainMenu.initial = 5;
            mainMenu.addItem("Single Player",MenuState.SinglePlayer);
            //mainMenu.addItem("Split Screen",MenuState.SplitScreen);
            //mainMenu.addItem("Options",MenuState.Options);
            mainMenu.addItem("Exit",MenuState.Exit);


            currentMenu = mainMenu;
            setCurrentMenu(MenuState.MainMenu);

            singlePlayerMenu = new Menu(MenuState.MainMenu);     
            //singlePlayerMenu.offset = new Vector2(150, 50);            
            refreshSPMenu();
            
            selectMapMenu = new Menu(MenuState.SinglePlayer);
            selectMapMenu.maxDist = 100;
            selectMapMenu.level = 2;
            //selectMapMenu.offset = new Vector2(150, 50);
            selectMapMenu.addItem("Shipment", MenuState.Null);
            selectMapMenu.addItem("Backlot", MenuState.Null);

            botNumberMenu = new Menu(MenuState.SinglePlayer);
            botNumberMenu.maxDist = 100;
            botNumberMenu.level = 2;
            //botNumberMenu.offset = new Vector2(150, 50);          
            refreshMaxBots(7);
            Console.WriteLine("menu engine initialized.");
        }

        public void Update()
        {
            Vector2 selectVector = coreEngine.inputEngine.getLook();
            //coreEngine.DrawTextDebug("" + selectVector.ToString());
            Update(coreEngine.currentState, coreEngine.inputEngine.getMenuKeys(), selectVector-currentMenu.offset);           
            
            
            
            
           
        }

        public void Update(GameState current, List<MenuAction> menuActions, Vector2 selectVector)
        {

            currentMenu.reposition();

            foreach(MenuAction m in menuActions)
                Console.WriteLine(m);
            
            if (currentMenu.dist < currentMenu.maxDist)
            {
                currentMenu.dist += 30;

                if (currentMenu.dist > currentMenu.maxDist)
                    currentMenu.dist = currentMenu.maxDist;


               
            }
            //Console.WriteLine("DISTANCE:" + currentMenu.dist);


            if (menuActions.Contains(MenuAction.Up))
            {
                if (currentMenu.selectedIndex > 0)
                {
                    currentMenu.setSelected(currentMenu.selectedIndex - 1);
                    Vector2 newMousePos = currentMenu.getItems()[currentMenu.selectedIndex].position;
                    Mouse.SetPosition(400, (int)newMousePos.Y);
                }
            }
            else if (menuActions.Contains(MenuAction.Down))
            {
                if (currentMenu.selectedIndex < (int)currentMenu.getItems().Length)
                {
                    currentMenu.setSelected(currentMenu.selectedIndex + 1);
                    Vector2 newMousePos = currentMenu.getSelectedItem().position;
                    Mouse.SetPosition(400, (int)newMousePos.Y);
                }
            }
            else if (menuActions.Contains(MenuAction.Select))
            {
                setCurrentMenu(currentMenu.getSelectedItem().nextMenu);

            }
            else if (menuActions.Contains(MenuAction.Back))
            {
                setCurrentMenu(currentMenu.previousMenu);
            }
            else
            {
                selectAngle = (float)Math.Atan2((double)selectVector.Y, (double)selectVector.X);

                float initialAngle = (float)MathHelper.ToRadians(currentMenu.initial);

                int selectIndex = (int)((selectAngle - initialAngle + currentMenu.increment / 2) / currentMenu.increment);
                currentMenu.setSelected(selectIndex);
            }

           

        }
        public void setCurrentMenu(MenuState m)
        {
            
            currentMenu.dist = 0;
            //setOverlayMode = false;

            //Console.WriteLine(m);
            if (!setOverlayMode)
                menuStack.Clear();

            menuStack.Insert(0, currentMenu.getItems());   

            switch (m)
            {
              
                case MenuState.MainMenu:                    
                    currentMenu = mainMenu;
                    setOverlayMode = false;
                    break;
                case MenuState.SinglePlayer:
                    currentMenu = singlePlayerMenu;
                    setOverlayMode = false;
                    break;
                case MenuState.StartGame:
                    startGame();                  
                    break;
                case MenuState.SelectMap:
                    setOverlayMode = true;
                    currentMenu = selectMapMenu;
                    //Load map list from map engine
                    break;
                case MenuState.BotOptions:
                    setOverlayMode = true;
                    currentMenu = botNumberMenu;
                    break;
                case MenuState.SplitScreen:
                    setOverlayMode = false;
                    Console.WriteLine("SplitScreen Selected from arc");
                    break;
                case MenuState.Options:
                    setOverlayMode = false;
                    Console.WriteLine("Options Selected from arc");
                    break;
                case MenuState.Exit:
                    coreEngine.Exit();
                    break; 
                //when storing variable changes
                case MenuState.Null:
                    if (currentMenu == selectMapMenu)
                    {
                        //store name of chosen map (selected item)
                    }
                    if (currentMenu == botNumberMenu)
                    {
                        //store name of chosen number of bots
                        selectedNumBots= Int16.Parse(currentMenu.getSelectedItem().label);
                        refreshSPMenu();
                    }
                    setCurrentMenu(currentMenu.previousMenu);
                    break;



                 
            }

            //Console.WriteLine("--------------\n"+menuStack.Count);

            if (!setOverlayMode)
                menuStack.Clear();
            else if (menuStack.Count >= currentMenu.level)
            {
                menuStack.RemoveAt(0);
                menuStack.RemoveAt(0);
                //Console.WriteLine(menuStack.Count);
            }

            if (currentMenu.getItems()[0].textVar == null)
            {
                foreach (MenuItem i in currentMenu.getItems())
                {
                    i.set3DText(vecFont.Extrude(i.label));

                }
            }
            menuStack.Insert(0, currentMenu.getItems());
            //Console.WriteLine(menuStack.Count);

        }


        public void refreshSPMenu()
        {
            singlePlayerMenu.clear();
            singlePlayerMenu.addItem("Play Now", MenuState.StartGame);
            singlePlayerMenu.addItem("Select Map", MenuState.SelectMap);
            singlePlayerMenu.addItem("Number of Bots: " + selectedNumBots, MenuState.BotOptions);

        }


        public void refreshMaxBots(int n)
        {
            maxPlayersInMap = n;
            botNumberMenu.clear();

            for (int i = 1; i <= maxPlayersInMap; i++)
            {
                botNumberMenu.addItem(i.ToString(), MenuState.Null);
            }
        }



        public void startGame()
        {
            coreEngine.currentState = GameState.GameScreen;

        }


        public MenuItem[] getItems()
        {
            List<MenuItem> toDraw = new List<MenuItem>();

            //Console.WriteLine(menuStack.Count);
            

            foreach (MenuItem[] scope in menuStack)
            {
                foreach (MenuItem m in scope)
                {
                    toDraw.Add(m);
                }
            }        

            return toDraw.ToArray();
        }

        public void Draw(GameTime gametime)
        {
            coreEngine.GraphicsDevice.Clear(Color.Gray);
            DrawModel(coreEngine.cogModel, Matrix.CreateScale(2.5f) * Matrix.CreateRotationY((float)Math.Atan2(coreEngine.inputEngine.getLook().X, coreEngine.inputEngine.getLook().Y)) * Matrix.CreateRotationX(MathHelper.PiOver2));
            MenuItem[] items = getItems();

            textBatch.Begin();
            foreach (MenuItem m in items)
            {
                Matrix textTransform = Matrix.CreateTranslation(m.position.X, -m.position.Y, 0) * Matrix.CreateScale(new Vector3(1, 1, 3));
                textBatch.ViewProjection = view * projection;
                if (m.selected)               
                    textBatch.DrawText(m.textVar, textTransform * Matrix.CreateTranslation(new Vector3(0, 0, 20)), Color.Red); 
                else                
                    textBatch.DrawText(m.textVar, textTransform, Color.White);
            }
            textBatch.End();

          


        }

        public void DrawModel(Model model, Matrix world)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    effect.TextureEnabled = true;
                    effect.Texture = coreEngine.cogTexture;
                   
                    effect.World = transforms[mesh.ParentBone.Index] * world;

                    // Use the matrices provided by the chase camera
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }
    }
}