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

        public override void fire(Agent p, PhysicsEngine ph) {

            base.fire(p, ph);

            if (curCooldown == cooldown)
            {
                List<Agent> l = new List<Agent>();
                l.Add(p);
                PhysicsEngine.HitScan hs = ph.hitscan(p.getPosition() + new Vector3(0, 75, 0) + p.getDirectionVector() * 10, p.getDirectionVector(), null);
                PhysicsEngine.AgentHitScan ahs = ph.agentHitscan(p.getPosition() + new Vector3(0, 60, 0) + p.getDirectionVector() * 10, p.getDirectionVector(), l);
                if (hs != null && (ahs == null || hs.Distance() < ahs.Distance())) {
                    makeLaser(p, hs.ray, Vector3.Distance(hs.ray.Position, hs.collisionPoint), 5, 5, "Pistol");
                }
                else if (ahs != null) {
                    ahs.agent.dealDamage(damage, p);
                    makeLaser(p, ahs.ray, Vector3.Distance(ahs.ray.Position, ahs.collisionPoint), 5, 5, "Pistol");
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