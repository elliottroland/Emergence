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
    public abstract class MenuScreen
    {
        public MenuEngine menuEngine;
        public List<MenuItem> menuItems = new List<MenuItem>();
        public List<MenuAction> menuActions = new List<MenuAction>();
        public CoreEngine coreEngine;

        public static Texture2D A_button, B_button, X_button, Y_button;
        public static Texture2D LT_button, RT_button, LB_button, RB_button;
        public static Texture2D LS_button, RS_button;

        public static Texture2D background, light, selectWheel,splitWheel,loadWheel, title, line;
        public static Texture2D ugLeft, ugRight;
        public static Texture2D splatter1, splatter2;
        public static Texture2D crossHair;

        //Screen Params        
        public static int screenWidth = 1024;
        public static int screenHeight = 576;
        public static Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);

        //Menu Params
        public static float dist = 0;
        public static float maxDist = 560;
        public static float range = 40;
        public static Vector2 selectWheelPos = new Vector2(-300.0f, screenCenter.Y);
        public static Vector2 selectWheelDesiredPos = new Vector2(-100.0f, screenCenter.Y);
        public String type = "nonTitle";

        public float selectWheelRot = 0;

        public int selectIndex = 0;


      
        public abstract MenuScreen Update(GameTime g);
        public abstract void Draw();

        public void getInput() {
            getInput(false);
        }

        public void getInput(bool doubleFix)
        {
            if (type != "Title")
            {
                //Get select vector and wheel rotation

                //Console.WriteLine("Core: "+ coreEngine);
                //Console.WriteLine("Input: " + coreEngine.inputEngine);

                Vector2 ControllerSelect = coreEngine.inputEngine.getMove(PlayerIndex.One) * 250;
                ControllerSelect.Y = -ControllerSelect.Y;
                if (ControllerSelect != Vector2.Zero)
                    ControllerSelect.Y += 450;
                Vector2 selectVector = coreEngine.inputEngine.getLook() + new Vector2(ControllerSelect.X, ControllerSelect.Y);
                selectWheelRot = (float)Math.Atan2(selectVector.X, -selectVector.Y);



                //Selecting proper Item
                SelectedStruct x = updateSelected(menuItems, coreEngine, selectVector);//reposition
                for (int i = 0; i < menuItems.Count; i++)
                    menuItems[i].position = x.repositionItems[i].position;

            
                menuActions = coreEngine.inputEngine.getMenuKeys();
                List<MenuAction> menuActionsController1 = coreEngine.inputEngine.getMenuButtons(PlayerIndex.One);
                //Console.WriteLine(selectIndex);

                //Console.WriteLine(type);
                foreach (MenuAction m in menuActionsController1)
                    menuActions.Add(m);
            }
        }

        public static void loadButtonTextures(CoreEngine g)
        {
            A_button = g.Content.Load<Texture2D>("MenuTextures/ButtonTextures/button_a");
            B_button = g.Content.Load<Texture2D>("MenuTextures/ButtonTextures/button_b");
            X_button = g.Content.Load<Texture2D>("MenuTextures/ButtonTextures/button_x");
            Y_button = g.Content.Load<Texture2D>("MenuTextures/ButtonTextures/button_y");
            LT_button = g.Content.Load<Texture2D>("MenuTextures/ButtonTextures/trigger_left");
            RT_button = g.Content.Load<Texture2D>("MenuTextures/ButtonTextures/trigger_right");
            LB_button = g.Content.Load<Texture2D>("MenuTextures/ButtonTextures/bumper_left");
            RB_button = g.Content.Load<Texture2D>("MenuTextures/ButtonTextures/bumper_right");
            LS_button = g.Content.Load<Texture2D>("MenuTextures/ButtonTextures/stick_left");
            RS_button = g.Content.Load<Texture2D>("MenuTextures/ButtonTextures/stick_right");

            background = g.Content.Load<Texture2D>("MenuTextures/background2");
            selectWheel = g.Content.Load<Texture2D>("MenuTextures/SelectionWheel2");
            title = g.Content.Load<Texture2D>("MenuTextures/EmergenceIntro2");
            splitWheel = g.Content.Load<Texture2D>("MenuTextures/splitScreenWheel");
            line = g.Content.Load<Texture2D>("MenuTextures/line");
            loadWheel = g.Content.Load<Texture2D>("MenuTextures/loadWheel");

            ugLeft = g.Content.Load<Texture2D>("WeaponIcons/hug_upgrade_blue");
            ugRight = g.Content.Load<Texture2D>("WeaponIcons/hug_upgrade_red");

            splatter1 = g.Content.Load<Texture2D>("WeaponIcons/screenSplatter1");
            splatter2 = g.Content.Load<Texture2D>("WeaponIcons/screenSplatter2");

            crossHair = g.Content.Load<Texture2D>("WeaponIcons/crosshair");


        }

        public SelectedStruct updateSelected(List<MenuItem> menuItems, CoreEngine coreEngine, Vector2 selectVector)
        {
            SelectedStruct x = new SelectedStruct();
            x.repositionItems = reposition(menuItems);

            Vector2 wheelDirection = selectWheelDesiredPos - selectWheelPos;
            if (wheelDirection.LengthSquared() < 20)
                selectWheelPos += wheelDirection / 1000;
            else
                selectWheelPos = selectWheelDesiredPos;



            if (dist < maxDist)
                dist+=20;
            else if (dist > maxDist)
                dist -= 20;
            //set selected menuItem       

            float selectAngle = (float)Math.Atan2((double)selectVector.Y, (double)selectVector.X);
            float initialAngle = MathHelper.ToRadians(-15);
            float increment = MathHelper.ToRadians(range) / menuItems.Count;
            x.selectedIndex = (int)((selectAngle - initialAngle + increment / 2) / increment);
            setSelected(menuItems, x.selectedIndex); //setSelected(selectIndex);
            setSelectedIndex(x.selectedIndex);
           
            return x;

        }
        public void setSelectedIndex(int n)
        {
            if(n>=0&&n<menuItems.Count)
                selectIndex = n;

            if (selectIndex == 0 || selectIndex == 1) 
                selectIndex = 1;
            
        }

        public void drawTips()
        {
            
           
            coreEngine.spriteBatch.Draw(line, menuItems[0].position + new Vector2(-30, 16), null, Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 1f); 

            Vector2 startPos = new Vector2(screenWidth-160, screenHeight-145);
            Vector2 startButton = new Vector2(startPos.X + coreEngine.debugFont.MeasureString("Select").X, startPos.Y-10);
            Vector2 space = new Vector2(0, 45);
            coreEngine.spriteBatch.DrawString(coreEngine.debugFont, "Cursor", startPos, Color.White,0f,Vector2.Zero,0.6f,SpriteEffects.None,1f);
            coreEngine.spriteBatch.Draw(LS_button, startButton, null,Color.White,0f,Vector2.Zero,0.6f,SpriteEffects.None, 1f);            
            startPos += space;
            startButton += space;
            coreEngine.spriteBatch.DrawString(coreEngine.debugFont, "Select", startPos, Color.White,0f,Vector2.Zero,0.6f,SpriteEffects.None,1f);
            coreEngine.spriteBatch.Draw(A_button, startButton, null, Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 1f); 
            startPos += space;
            startButton += space;
            coreEngine.spriteBatch.DrawString(coreEngine.debugFont, "Back ", startPos, Color.White,0f,Vector2.Zero,0.6f,SpriteEffects.None,1f);
            coreEngine.spriteBatch.Draw(B_button, startButton, null, Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 1f); 
            startPos += space;
            startButton += space;



           

        }


        public List<MenuItem> setSelected(List<MenuItem> menuItems, int index)       
        {
            if (index < menuItems.Count && index>0)
            {
                for (int i = 0; i < menuItems.Count; i++)
                {
                    menuItems[i].selected = false;
                }
                //selectedIndex = index;
                menuItems[index].selected = true;
            }
            return menuItems;

        }

        public List<MenuItem> reposition(List<MenuItem> t)
        {
            double rangeRadians = MathHelper.ToRadians(range);
            float increment = (float)rangeRadians / menuItems.Count;
            double initialAngle = MathHelper.ToRadians(-15);
            for (int i = t.Count - 1; i >= 0; i--)
            {
                t[i].setPosition(dist * Math.Cos(i * increment + initialAngle) + selectWheelPos.X, dist * Math.Sin(i * increment + initialAngle) + selectWheelPos.Y);
                //Console.Write(i + " " + t[i].position);
            }
            return t;

        }

      
    }
}