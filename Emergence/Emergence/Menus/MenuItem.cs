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

namespace Emergence
{
    public class MenuItem
    {
        public String label = "";
        public Vector2 position = Vector2.Zero;
        public bool selected = false;
        public MenuState nextMenu;
        public Text textVar;

        public MenuItem(String n, MenuState next)
        {
            nextMenu = next;           
            label = n;            
        }

        public void set3DText(Text t)
        {
            textVar = t;
        }
        

        public void setPosition(double x, double y)
        {
            position = new Vector2((float)x, (float)y);
        }

    }
}
