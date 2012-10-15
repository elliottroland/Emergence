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
    public class Rifle : Weapon
    {
        float inaccuracy = 0, inaccruacyJump = 0.05f, maxInaccuracy = 0.125f, inaccuracyCooldownTime = 1, inaccuracyCurCooldown = 0;

        public Rifle()
        {
            ammoUsed = 5;
            cooldown = 0.15f;
            damage = 20;
            // TODO: Construct any child components here
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            inaccuracyCurCooldown = (float)Math.Min(inaccuracyCooldownTime, inaccuracyCurCooldown + gameTime.ElapsedGameTime.TotalSeconds);
            if(inaccuracyCurCooldown == inaccuracyCooldownTime)
                inaccuracy = Math.Max(inaccuracy - inaccruacyJump / 2, 0);
        }

        public override void fire(Player p, PhysicsEngine ph)
        {
            base.fire(p, ph);

            if (curCooldown == cooldown) {
                Random rand = new Random();
                Vector3 dir = Vector3.Normalize(p.getDirectionVector());
                Vector3 right = Vector3.Cross(dir, Vector3.Up);
                Vector3 up = Vector3.Cross(dir, right);
                up *= inaccuracy;
                right *= inaccuracy;
                dir = dir + (float)(rand.NextDouble() * 2 - 1) * up + (float)(rand.NextDouble() * 2 - 1) * right;
                inaccuracy = Math.Min(maxInaccuracy, inaccruacyJump + inaccuracy);
                inaccuracyCurCooldown = 0;

                PhysicsEngine.HitScan hs = ph.hitscan(p.position + new Vector3(0, 60, 0) + p.getDirectionVector() * 10, dir, null);
                if (hs != null) {
                    makeLaser(p, hs.ray, Vector3.Distance(hs.ray.Position, hs.collisionPoint), 5, 5);
                }
            }
        }

        public override Weapon upgradeLeft()
        {
            return new SniperRifle();
        }

        public override Weapon upgradeRight()
        {
            return new ShockRifle();
        }

        public override Weapon upgradeDown()
        {
            return new Pistol();
        }

    }
}