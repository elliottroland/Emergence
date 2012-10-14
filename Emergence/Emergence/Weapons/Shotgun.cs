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


namespace Emergence.Weapons
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Shotgun : Weapon
    {
        public Shotgun()
        {
            ammoUsed = 10;
            damage = 5;     //per bullet
            cooldown = 40;

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void fire(Player p, PhysicsEngine ph)
        {
            base.fire(p, ph);
        }

        public override Weapon upgradeLeft()
        {
            return new FlakCannon();
        }

        public override Weapon upgradeRight()
        {
            return new RocketLauncher();
        }

        public override Weapon upgradeDown()
        {
            return new Pistol();
        }

    }
}