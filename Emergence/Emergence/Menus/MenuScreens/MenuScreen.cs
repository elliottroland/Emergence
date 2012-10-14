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

        //Screen Params        
        public static int screenWidth = 1024;
        public static int screenHeight = 576;
        public static Vector2 screenCenter = new Vector2(screenWidth / 2, screenHeight / 2);

        //Menu Params
        public static float dist = 0;
        public static float maxDist = 560;
        public static float range = 40;
        public static Vector2 selectWheelPos = new Vector2(-100.0f, screenCenter.Y);
        public String type = "nonTitle";

        public float selectWheelRot = 0;

        public int selectIndex = 0;


        public abstract void LoadContent();
        public abstract MenuScreen Update(GameTime g);
        public abstract void Draw();

        public void getInput()
        {
            if (type != "Title")
            {
                //Get select vector and wheel rotation
                Console.WriteLine("Core: "+ coreEngine);
                Console.WriteLine("Input: " + coreEngine.inputEngine);
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

                Console.WriteLine(type);
                foreach (MenuAction m in menuActionsController1)
                    menuActions.Add(m);
            }
        }


        public SelectedStruct updateSelected(List<MenuItem> menuItems, CoreEngine coreEngine, Vector2 selectVector)
        {
            SelectedStruct x = new SelectedStruct();
            x.repositionItems = reposition(menuItems);

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

        public String ToString()
        {
            return " ";
        }
    }
}