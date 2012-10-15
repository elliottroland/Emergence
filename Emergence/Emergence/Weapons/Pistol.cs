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
    public class Pistol : Weapon
    {
        
        public Pistol()
        {

            cooldown = 0.75f;
            ammoUsed = 0;
            damage = 5;

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void fire(Player p, PhysicsEngine ph) {

            base.fire(p, ph);

            if (curCooldown == cooldown)
            {
                PhysicsEngine.HitScan hs = ph.hitscan(p.position+new Vector3(0, 60, 0)+p.getDirectionVector()*10, p.getDirectionVector(), null);
                if (hs != null)
                {
                    makeLaser(p, hs.ray, Vector3.Distance(hs.ray.Position, hs.collisionPoint), 5, 5);
                }
            }
        
        }

        public override Weapon upgradeLeft()
        {
            return new Rifle();
        }

        public override Weapon upgradeRight()
        {
            return new Shotgun();
        }

        public override Weapon upgradeDown() { return this; }

    }
}