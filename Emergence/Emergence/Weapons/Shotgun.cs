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
            cooldown = 1f;

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void fire(Player p, PhysicsEngine ph)
        {
            base.fire(p, ph);

            if (curCooldown == cooldown) {
                Random rand = new Random();
                Vector3 [] dirs = new Vector3[5];
                dirs[0] = Vector3.Normalize(p.getDirectionVector());
                Vector3 right = Vector3.Cross(dirs[0], Vector3.Up);
                Vector3 up = Vector3.Cross(dirs[0], right);
                up *= 0.15f;
                right *= 0.15f;
                dirs[1] = dirs[0] + (float)rand.NextDouble() * right + (float)rand.NextDouble() * up;
                dirs[2] = dirs[0] + (float)rand.NextDouble() * right - (float)rand.NextDouble() * up;
                dirs[3] = dirs[0] - (float)rand.NextDouble() * right - (float)rand.NextDouble() * up;
                dirs[4] = dirs[0] - (float)rand.NextDouble() * right + (float)rand.NextDouble() * up;

                foreach(Vector3 dir in dirs)    {
                    PhysicsEngine.HitScan hs = ph.hitscan(p.position + new Vector3(0, 60, 0) + p.getDirectionVector() * 10, dir, null);
                    if (hs != null) {
                        makeLaser(p, hs.ray, Vector3.Distance(hs.ray.Position, hs.collisionPoint), 5, 5);
                    }
                }
            }
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