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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public abstract class Weapon
    {

        public int damage;
        public int ammoUsed;
        public int cooldown;
        public int curCooldown;

        public Weapon()
        {
        }

        public virtual void Update(GameTime gameTime)
        {
            if (curCooldown > 0)
                curCooldown--;
        }

        public virtual void fire(Player p){

            if (curCooldown > 0 || p.ammo < ammoUsed)
                return;

            p.ammo -= ammoUsed;
            curCooldown = cooldown;

        }

        //Traverse the weapon by the left branch
        public abstract Weapon upgradeLeft();

        //Traverse the weapon by the right branch
        public abstract Weapon upgradeRight();

        //Revert to more basic weapon
        public abstract Weapon upgradeDown();

    }
}