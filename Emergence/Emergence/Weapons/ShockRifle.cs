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
    public class ShockRifle : Weapon
    {
        public ShockRifle()
        {
            ammoUsed = 5;
            cooldown = 1f;
            damage = 5;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void fire(Agent p, PhysicsEngine ph)
        {
            base.fire(p, ph);

            if (curCooldown == cooldown) {
                PhysicsEngine.HitScan hs = ph.hitscan(p.position + new Vector3(0, 60, 0) + p.getDirectionVector() * 10, p.getDirectionVector(), null);
                if (hs != null) {
                    makeProjectile(p, hs.ray, Vector3.Distance(hs.ray.Position, hs.collisionPoint), 60, 1000, 50, "ShockRifle");
                }
            }
        }

        public override Weapon upgradeLeft()
        {
            return this;
        }

        public override Weapon upgradeRight()
        {
            return this;
        }

        public override Weapon upgradeDown()
        {
            return new Rifle();
        }

    }
}