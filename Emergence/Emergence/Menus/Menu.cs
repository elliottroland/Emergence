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
    public class Menu
    {



        List<MenuItem> menuItems = new List<MenuItem>();
        public MenuState previousMenu;
        public int selectedIndex = -1;
        public double increment = 0;
        public int level = 1;
        public float rangeDegrees = 65;
        public float maxDist = 200;
        public float dist = 0;
        public Vector2 offset = new Vector2(0, 0);
        public float initial = 5;
        Vector2 center = Vector2.Zero;

        public Menu() { }

        public Menu( MenuState prev)
        {
            previousMenu = prev;
           
        }


        public void addItem(String s, MenuState next)
        {
            menuItems.Add(new MenuItem(s,next));

            reposition();

            
        }

        public void clear()
        {
            menuItems.Clear();

        }
        public MenuItem getSelectedItem()
        {
            return menuItems[selectedIndex];
        }

        public MenuItem[] getItems()
        {            
            return menuItems.ToArray();
        }


        public void setSelected(int index)
        {
            if (index < menuItems.Count && index >= 0)
            {
                for (int i = 0; i < menuItems.Count; i++)
                {
                    menuItems[i].selected = false;
                }
                selectedIndex = index;
                menuItems[index].selected = true;
            }

        }

        public void reposition()
        {
            double rangeRadians = MathHelper.ToRadians(rangeDegrees);
            increment = rangeRadians / menuItems.Count;
            double initialAngle = MathHelper.ToRadians(initial);
            for (int i = menuItems.Count - 1; i >= 0; i--)
            {
                menuItems[i].setPosition(dist * Math.Cos(i * increment + initialAngle) + offset.X, dist * Math.Sin(i * increment + initialAngle) + offset.Y);
            }

        }

      

       



    }
}