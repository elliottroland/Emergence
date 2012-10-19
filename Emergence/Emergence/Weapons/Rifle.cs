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
            damage = 10;
            // TODO: Construct any child components here
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            inaccuracyCurCooldown = (float)Math.Min(inaccuracyCooldownTime, inaccuracyCurCooldown + gameTime.ElapsedGameTime.TotalSeconds);
            if(inaccuracyCurCooldown == inaccuracyCooldownTime)
                inaccuracy = Math.Max(inaccuracy - inaccruacyJump / 2, 0);
        }

        public override void fire(Agent p, PhysicsEngine ph)
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

                List<Agent> l = new List<Agent>();
                l.Add(p);
                PhysicsEngine.HitScan hs = ph.hitscan(p.getPosition() + new Vector3(0, 75, 0) + p.getDirectionVector() * 10, p.getDirectionVector(), null);
                PhysicsEngine.AgentHitScan ahs = ph.agentHitscan(p.getPosition() + new Vector3(0, 60, 0) + p.getDirectionVector() * 10, dir, l);
                if (hs != null && (ahs == null || hs.Distance() < ahs.Distance()))
                    makeLaser(p, hs.ray, Vector3.Distance(hs.ray.Position, hs.collisionPoint), 5, 5, "Rifle");
                else if (ahs != null) {
                    ahs.agent.dealDamage(damage, p);
                    makeLaser(p, ahs.ray, Vector3.Distance(ahs.ray.Position, ahs.collisionPoint), 5, 5, "Rifle");
                }
            }
        }

        public override Weapon upgradeLeft()
        {
            return new Railgun();
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